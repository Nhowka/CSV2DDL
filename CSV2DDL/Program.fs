open System
open System.IO
type cr = FSharp.Data.CsvProvider<Sample=""""Name","Type","Nullable","Default","Storage","Comments"
"NROCARGARECEB","NUMBER(10)","N","","","" """>

let convert (folder:string) =
    let format =
        function
        |(var, ty, "Y", "") -> sprintf "%s %s" var ty
        |(var, ty, "Y", def) -> sprintf "%s %s DEFAULT %s" var ty def
        |(var, ty, _, "") -> sprintf "%s %s NOT NULL" var ty
        |(var, ty, _, def) -> sprintf "%s %s DEFAULT %s NOT NULL" var ty def
    let create ((table:string), content)=
        sprintf "CREATE TABLE %s (\n    %s\n);" (table.ToUpper()) content
    let fixType (ty:string)=
        ty.Replace("NUMBER","NUMERIC").Replace("VARCHAR2","VARCHAR")
    Directory.GetFiles(folder, "*.csv")
    |> Array.map
        (fun f ->
        use tr = File.OpenText(f)
        let fi = FileInfo(f)

        tr.ReadToEnd() |> cr.Parse
        |>(fun c -> (fi.Name |> Seq.takeWhile ((<>)'.')|> Seq.toArray |> String, c.Rows |> Seq.map (fun r-> (r.Name, fixType r.Type, r.Nullable, r.Default) |> format) |>String.concat",\n    "))|> create)
    |> String.concat "\n\n"

[<EntryPoint>]
let main argv =
    let folder =
        match argv with
        |[||] -> Directory.GetCurrentDirectory()
        |_ -> argv.[0]
    convert folder |>
    printfn "%s"
    0 // return an integer exit code