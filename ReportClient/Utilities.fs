namespace ReportClient

open System
open System.Text
open System.IO
open System.Runtime.InteropServices
open System.Security.Cryptography
open System.Globalization
open Microsoft.FSharp.Reflection

module Utilities =
  let md5Hash (text : string) =
   use algo  = new MD5CryptoServiceProvider ()
   algo.ComputeHash(Encoding.ASCII.GetBytes text)
   |> Array.map (fun x -> x.ToString "x2")
   |> String.concat ""

  let toString (x:'a) = 
      let (case, _ ) = FSharpValue.GetUnionFields(x, typeof<'a>)
      case.Name
  
  let fromString<'a> (s:string) =
    match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
    |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
    |_ -> None