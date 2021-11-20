namespace ReportClient

open System
open Newtonsoft.Json
open System.IO
open LemonCypher
open LemonCypher

type Config = 
  { Server: string
    Token : string
    CustomerId : string
    Version : string }

type ConfigEx = //Possible errors while reading config file
  | FileNotFound 
  | JsonParseError
  | InvalidKeyFormat

module Config =
  let aes = aesGen "5f042e659d16ada62bd286483a753db6"

  ///<summary> Reads, deserializes, and decrypts the key from json config file. </summary>
  let read () =
    try
      let config =
        use stream = new StreamReader("MyItracker.json")
        stream.ReadToEnd ()
        |> JsonConvert.DeserializeObject<Config>
      Ok {config with Token = decrypt aes config.Token } //Returns a config object with key deserialized Hack implement encryption
    with
    | :? FileNotFoundException -> Error FileNotFound
    | :? JsonException | :? InvalidCastException -> Error JsonParseError
    | :? FormatException -> Error InvalidKeyFormat