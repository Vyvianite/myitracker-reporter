namespace ReportClient

open System
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices
open System.Text.RegularExpressions

[<Struct;StructLayout(LayoutKind.Sequential)>]
type PerfInfo =
  val mutable Size : int
  val mutable CommitTotal : IntPtr
  val mutable CommitLimit : IntPtr
  val mutable CommitPeak : IntPtr
  val mutable PhysicalTotal : IntPtr
  val mutable PhysicalAvailable : IntPtr
  val mutable SystemCache : IntPtr
  val mutable KernelTotal : IntPtr
  val mutable KernelPaged : IntPtr
  val mutable KernelNonPaged : IntPtr
  val mutable PageSize : IntPtr
  val mutable HandlesCount : int
  val mutable ProcessCount : int
  val mutable ThreadCount :int

type DriveData = 
  { Name : string
    Total : int
    Free : int
    Used : int }

type MemoryData = 
  { Total : int
    Free : int
    Used : int }

type Report =
  { Processes : string list
    Events : string list
    Drives : DriveData list
    Memory : MemoryData
    OSData : OSData
    ComputerName : string
    ComputerId : string }
    
[<AbstractClass>] (*Base type provides platform non-specific functions for submodules*)
type Reporter (os) =

  (*Never empty*)
  member __.Processes () =
    [ for p in Process.GetProcesses() -> //todo Test for proper exception handling
        try
          Some p.ProcessName
        with
        | invalidOp -> None //On mac reaching the end of the array can cause an exception.
    ]
    |> List.choose id

  member __.Drives () =
    try
      DriveInfo.GetDrives ()
      |> Array.toList
      |> List.map 
          ( fun x -> 
              if x.IsReady then
                Some
                  { Name = x.Name
                    Total = x.TotalSize / 1_073_741_824L |> int
                    Free = x.TotalFreeSpace / 1_073_741_824L |> int
                    Used = (x.TotalSize - x.TotalFreeSpace) / 1_073_741_824L |> int }
              else
                None )
      |> List.choose id
    with 
    | _ ->
      [{Name = "Error"; Total = -1; Used = -1; Free = -1}]

  abstract Events : unit -> string list
  abstract Memory : unit -> MemoryData

  member  this.Collate () : Report =
    { Processes = this.Processes ()
      Events = this.Events ()
      Drives = this.Drives ()
      Memory = this.Memory ()
      OSData = os
      ComputerName = Environment.MachineName
      ComputerId = Utilities.md5Hash (Environment.MachineName) }

type private WindowsReporter (os) =
  inherit Reporter (os)

  [<DllImport("psapi.dll", SetLastError = true)>] (*This is used for getting windows memory.*)
  static extern [<return: MarshalAs(UnmanagedType.Bool)>] bool GetPerformanceInfo(PerfInfo& PerformanceInformation, int Size) //todo Test me. Does Size need to be an inref<>?

  override __.Events () =
    let timeBound = DateTime.UtcNow.AddDays(-1.0)
    use events = 
      new EventLog(
        Log = "Application",
        MachineName = "."
      )
    [ for i in events.Entries ->
        if (i.TimeGenerated > timeBound && Regex.IsMatch(i.Message, "error", RegexOptions.IgnoreCase)) then
          Some (i.Message)
        else
          None
    ]
    |> List.choose id

  override __.Memory () =
    let mutable pi = PerfInfo()
    if GetPerformanceInfo(&pi, Marshal.SizeOf(pi)) then
      let total = Convert.ToInt64(pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1_048_576L)
      let free = Convert.ToInt64(pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1_048_576L)
      { Total = total |> int
        Free = free |> int
        Used = total - free |> int }
    else
      {Total = -1; Free = -1; Used = -1}

type private MacReporter (os) =
  inherit Reporter (os)

  override __.Events () =
    let logFile = FileInfo @"/var/log/system.log" //hack Magic string or is this fine?
    match logFile.Exists with
    | true -> //todo Implement filtering and reading each line into a seq
        //use reader = new StreamReader (logFile.FullName)
        //let events = reader.ReadToEnd () 
        //if not (isNull events) then
        //  events
        //else
          //"None"
        ["Filtering Not Implemented"] //hack Temp return until I figure out how to 
    | false ->
        ["Error"]

  override __.Memory () =
    let pageToMB pages = 
      pages 
      * 4_096L //Bytes
      / 1_024L //KB
      / 1_024L //MB

    try
      use myProcess = new Process()
      myProcess.StartInfo.UseShellExecute <- false
      myProcess.StartInfo.RedirectStandardOutput <- true
      myProcess.StartInfo.RedirectStandardError <- true
      myProcess.StartInfo.FileName <- "vm_stat"
      myProcess.StartInfo.CreateNoWindow <- true
      myProcess.Start() |> ignore
      let output = 
        myProcess.StandardOutput.ReadToEnd()
          .Replace(" ", String.Empty)
          .Replace(".", String.Empty)
          .Split([|"\r\n"; "\r"; "\n"|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
        |> List.skip 1
        |> List.map 
            ( fun x ->
                let split = x.Split(':')
                let parsed = int64 <| split.[1]
                (split.[0], parsed) )
        |> Map.ofList

      let total = 
        output.["Pagesfree"] 
        + output.["Pagesinactive"] 
        + output.["Pagesactive"] 
        + output.["Pagesspeculative"] 
        + output.["Pageswireddown"] 
        + output.["Pagespurgeable"]

      let used = 
        output.["Pagesactive"] 
        + output.["Pagesspeculative"] 
        + output.["Pageswireddown"] 
        + output.["Pagespurgeable"]

      let free = 
        output.["Pagesfree"] 
        + output.["Pagesinactive"]

      { Total = pageToMB total |> int
        Free = pageToMB free |> int
        Used = pageToMB used |> int }
    with
    | _ -> 
      {Total = -1; Free = -1; Used = -1}

module Reporter =

  ///<summary> Provides platform specific parameterized reporting module. </summary>
  let create os =
    match os with
    | Windows x -> WindowsReporter x :> Reporter
    | Mac x -> MacReporter x :> Reporter