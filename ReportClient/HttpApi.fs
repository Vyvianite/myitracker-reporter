namespace ReportClient

open System
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json

module HttpApi =
  let await = Async.AwaitTask
  let task = Async.StartAsTask
  let client = new HttpClient() 
  client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))

  let post (uri : Uri) command items =
    async {
      try
        use content = new MultipartFormDataContent()

        content.Add(new StringContent(command), "command")

        match items with
        | Some x -> x |> Map.iter(fun key value -> content.Add(new StringContent(value), key))
        | None -> ()

        let! response = await <| client.PostAsync(uri, content)
        let! responseString = await <| response.Content.ReadAsStringAsync()
        return Ok responseString
      with
      | _ as x -> return Error (sprintf "Network Error: %s; " x.Message)
    }
    
  let json<'t> uri command items =
    async {
      let! response = post uri command items
      return
        match response with 
          | Ok x -> 
            try
              Ok (JsonConvert.DeserializeObject<'t>(x))
            with
            | :? JsonException | :? InvalidCastException as z -> 
              Error (sprintf "Json Parsing Error: Json String= %s; Exception: %s; " x z.Message)
          | Error x -> Error x 
    }