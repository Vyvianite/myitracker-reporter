



namespace ReportClient
  type OSData =
    { Name: string
      Description: string
      Version: string }
  type OSType =
    | Windows of OSData
    | Mac of OSData
    with
      member unwrap : unit -> OSData
    end
  module OSType = begin
    val private osFactory : name:string -> OSData
    val check : unit -> Result<OSType,string>
  end

namespace ReportClient
  type Logger =
    class
      new : os:OSType -> Logger
      member log : text:string * ?finished:'a -> unit
    end

namespace ReportClient
  module Utilities = begin
    val md5Hash : text:string -> string
    val toString : x:'a -> string
    val fromString : s:string -> 'a option
  end

namespace LemonCypher
  module LemonCypher = begin
    val aesGen : password:string -> System.Security.Cryptography.AesGcm
    val encrypt :
      aes:System.Security.Cryptography.AesGcm -> plain:string -> string
    val decrypt :
      aes:System.Security.Cryptography.AesGcm -> cipher:string -> string
  end

namespace ReportClient
  type Config =
    { Server: string
      Token: string
      CustomerId: string
      Version: string }
  type ConfigEx =
    | FileNotFound
    | JsonParseError
    | InvalidKeyFormat
  module Config = begin
    val aes : System.Security.Cryptography.AesGcm
    val read : unit -> Result<Config,ConfigEx>
  end

namespace ReportClient
  [<StructAttribute ()>]
  type PerfInfo =
    struct
      val mutable Size: int
      val mutable CommitTotal: System.IntPtr
      val mutable CommitLimit: System.IntPtr
      val mutable CommitPeak: System.IntPtr
      val mutable PhysicalTotal: System.IntPtr
      val mutable PhysicalAvailable: System.IntPtr
      val mutable SystemCache: System.IntPtr
      val mutable KernelTotal: System.IntPtr
      val mutable KernelPaged: System.IntPtr
      val mutable KernelNonPaged: System.IntPtr
      val mutable PageSize: System.IntPtr
      val mutable HandlesCount: int
      val mutable ProcessCount: int
      val mutable ThreadCount: int
    end
  type DriveData =
    { Name: string
      Total: int
      Free: int
      Used: int }
  type MemoryData =
    { Total: int
      Free: int
      Used: int }
  type Report =
    { Processes: string list
      Events: string list
      Drives: DriveData list
      Memory: MemoryData
      OSData: OSData
      ComputerName: string
      ComputerId: string }
  [<AbstractClassAttribute ()>]
  type Reporter =
    class
      new : os:OSData -> Reporter
      member Collate : unit -> Report
      member Drives : unit -> DriveData list
      abstract member Events : unit -> string list
      abstract member Memory : unit -> MemoryData
      member Processes : unit -> string list
    end
  type private WindowsReporter =
    class
      inherit Reporter
      new : os:OSData -> WindowsReporter
      override Events : unit -> string list
      override Memory : unit -> MemoryData
    end
  type private MacReporter =
    class
      inherit Reporter
      new : os:OSData -> MacReporter
      override Events : unit -> string list
      override Memory : unit -> MemoryData
    end
  module Reporter = begin
    val create : os:OSType -> Reporter
  end

namespace ReportClient
  module HttpApi = begin
    val await : arg00:System.Threading.Tasks.Task<'a> -> Async<'a>
    val task : arg00:Async<'a> -> System.Threading.Tasks.Task<'a>
    val client : System.Net.Http.HttpClient
    val post :
      uri:System.Uri ->
        command:string ->
          items:Map<string,string> option -> Async<Result<string,string>>
    val json :
      uri:System.Uri ->
        command:string ->
          items:Map<string,string> option -> Async<Result<'t,string>>
  end

namespace ReportClient
  type Commander =
    interface
      abstract member restart : unit -> bool
      abstract member shutdown : unit -> bool
    end
  type InCommand =
    { command: string }
  module Commands = begin
    val create : os:OSType -> Commander
    type Command =
      | Shutdown
      | Restart
      with
        static member fromstring : command:string -> Command option
      end
    val run : commander:Commander -> command:string -> bool option
  end

namespace ReportClient
  module Program = begin
    val main : args:string [] -> int
  end

