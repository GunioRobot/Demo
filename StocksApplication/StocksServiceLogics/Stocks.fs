﻿module Stocks
open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Runtime.Serialization
open System.Net

[<DataContract>]
type StockQuote = {
    [<DataMember>] mutable Date : DateTime
    [<DataMember>] mutable Rate : double
    }

let public MakeUrl symbol (dfrom:DateTime) (dto:DateTime) =
    let monthfix (d:DateTime)= (d.Month-1).ToString()
    new Uri("http://ichart.finance.yahoo.com/table.csv?s=" + symbol +
        "&e=" + dto.Day.ToString() + "&d=" + monthfix(dto) + "&f=" + dto.Year.ToString() +
        "&g=d&b=" + dfrom.Day.ToString() + "&a=" + monthfix(dfrom) + "&c=" + dfrom.Year.ToString() +
        "&ignore=.csv")

let internal fetch (url : Uri) =
    let req:HttpWebRequest = downcast WebRequest.Create (url)
    use stream = req.GetResponse().GetResponseStream()
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

let internal decompose (response:string) =
    let split (mark:char) (data:string) =
        data.Split(mark) |> Array.toList
    response |> split '\n'
    |> List.filter (fun f -> f<>"")
    |> List.map (split ',')

let internal reformat (sel) =
    let parseDate d = DateTime.ParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture)
    let parseRate r = Double.Parse(r, CultureInfo.GetCultureInfo("en-US"))
    let focus (l:string list) = { Date = parseDate l.[0]; Rate = parseRate l.[4] }
    Seq.skip 1 sel |> Seq.map focus

let public GetResult url = (fetch >> decompose >> reformat) url
//let req = MakeUrl "MSFT" (new DateTime(2010, 2, 20)) (new DateTime(2010, 3, 25))
