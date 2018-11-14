/// Translates words missing from languages using
/// Microsoft's Cognitive Services

#r "Newtonsoft.Json.dll"
#r "System.Net.Http"

open System
open System.IO
open System.Net.Http
open System.Text
open Newtonsoft.Json

let cognitiveKey = System.IO.File.ReadAllText("key").Trim()

type Language = { Name : string; Microsoft : string; Apple : string }

let langs = [
    { Name = "English"; Microsoft = "en"; Apple = "Base" }
    { Name = "Chinese Simplified"; Microsoft = "zh-Hans"; Apple = "zh-Hans" }
    { Name = "Chinese Traditional"; Microsoft = "zh-Hant"; Apple = "zh-Hant" }
    { Name = "French"; Microsoft = "fr"; Apple = "fr" }
    { Name = "German"; Microsoft = "de"; Apple = "de" }
    { Name = "Italian"; Microsoft = "it"; Apple = "it" }
    { Name = "Japanese"; Microsoft = "ja"; Apple = "ja" }
    { Name = "Russian"; Microsoft = "ru"; Apple = "ru" }
    { Name = "Spanish"; Microsoft = "es"; Apple = "es" }
    { Name = "Portuguese"; Microsoft = "pt"; Apple = "pt" }
    ]

type TranslationReq = { Text : string }
type Translation = { Text : string; To : string }
type Translations = { Translations : Translation list }

let client = new HttpClient ()

let translate (texts : string seq) =
    let host = "https://api.cognitive.microsofttranslator.com"
    let path = "/translate?api-version=3.0"
    let params_ = "&from=en&" + String.Join ("&", langs |> List.map (fun x -> x.Microsoft) |> List.map (sprintf "to=%s"))
    let uri = Uri (host + path + params_)
    let translateChunk (chunk : string seq) =
        let requestBody = chunk |> Seq.map (fun text -> { Text = text }) |> JsonConvert.SerializeObject
        use request = new HttpRequestMessage (Method = HttpMethod.Post,
                                              RequestUri = uri,
                                              Content = new StringContent (requestBody, Encoding.UTF8, "application/json"))
        request.Headers.Add ("Ocp-Apim-Subscription-Key", cognitiveKey)
        let response = client.SendAsync(request).Result
        let responseBody = response.Content.ReadAsStringAsync().Result
        JsonConvert.DeserializeObject<Translations[]> (responseBody)
        |> Seq.zip chunk
        |> Seq.map (fun (en, ts) -> (en, ts.Translations |> Seq.map (fun x -> (x.To, x.Text)) |> Map.ofSeq))
    texts |> Seq.chunkBySize 25 |> Seq.collect translateChunk |> Map.ofSeq

let stringsPath lang =
    let dir = lang.Apple + ".lproj"
    Path.Combine (dir, "Localizable.strings")

let path = stringsPath langs.Head

let readStrings path =
    if File.Exists path then
        File.ReadAllLines (path, Encoding.UTF8)
        |> Seq.map (fun x -> x.Trim ())
        |> Seq.filter (fun x -> x.Length > 0)
        |> Seq.map (fun x -> x.Split ('\"'))
        |> Seq.filter (fun x -> x.Length = 5)
        |> Seq.map (fun x -> x.[1].Replace("\\n","\n").Trim (), x.[3].Replace("\\n","\n").Trim ())
        |> Seq.filter (fun (_, x) -> x.Length > 0)
        |> Map.ofSeq
    else
        Map.empty

let langStrings = langs |> Seq.map (fun x -> x, x |> stringsPath |> readStrings) |> Map.ofSeq

let uniqueKeys =
    langStrings
    |> Seq.collect (fun x -> x.Value |> Seq.map (fun x -> x.Key))
    |> Seq.distinct
    |> Seq.sort
    |> Array.ofSeq

let needTranslateKeys =
    // uniqueKeys
    let langIsMissingKey key =
       langs
       |> Seq.exists (fun x -> langStrings.[x].ContainsKey key |> not)
    uniqueKeys |> Array.filter langIsMissingKey

let translations = needTranslateKeys |> translate

Console.OutputEncoding <- UnicodeEncoding.UTF8
let conflict lang (key : string) (oldt : string) (newt : string) =
    Console.Write "Conflict "
    Console.ForegroundColor <- ConsoleColor.Yellow
    Console.Write "en="
    Console.Write key
    Console.Write " "
    Console.ForegroundColor <- ConsoleColor.Cyan
    Console.Write lang.Apple
    Console.Write "="
    Console.Write oldt
    Console.Write " "
    Console.ForegroundColor <- ConsoleColor.Magenta
    Console.Write lang.Apple
    Console.Write "="
    Console.WriteLine newt
    Console.ResetColor ()
let merge lang (key : string) (newt : string) =
    Console.Write "Merge "
    Console.ForegroundColor <- ConsoleColor.Yellow
    Console.Write "en="
    Console.Write key
    Console.Write " "
    Console.ForegroundColor <- ConsoleColor.Green
    Console.Write lang.Apple
    Console.Write "="
    Console.WriteLine newt
    Console.ResetColor ()

let translatedLangStrings =
    langs
    |> Seq.map (fun lang ->
        let s = langStrings.[lang]
        lang, uniqueKeys
        |> Seq.choose (fun k ->
            if s.ContainsKey k then
                if translations.ContainsKey k && (s.[k].Equals (translations.[k].[lang.Microsoft], StringComparison.InvariantCultureIgnoreCase) |> not) then
                    conflict lang k s.[k] translations.[k].[lang.Microsoft]
                (k, s.[k]) |> Some
            elif translations.ContainsKey k then
                merge lang k translations.[k].[lang.Microsoft]
                (k, translations.[k].[lang.Microsoft]) |> Some
            else None)
        |> Map.ofSeq)
    |> Map.ofSeq

let writeTranslations lang =
    let path = stringsPath lang
    let dir = Path.GetDirectoryName path
    if Directory.Exists dir |> not then
        Directory.CreateDirectory (dir) |> ignore
    use w = new StreamWriter (path, false, Encoding.UTF8)
    for k in uniqueKeys do
        if translatedLangStrings.[lang].ContainsKey k then
            w.WriteLine (sprintf "\"%s\" = \"%s\";" k translatedLangStrings.[lang].[k])

langs |> Seq.iter writeTranslations
