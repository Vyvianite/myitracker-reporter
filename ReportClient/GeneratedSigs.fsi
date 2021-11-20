



namespace ReportClient
    
    type OSData =
        {
          Name: string
          Description: string
          Version: string
        }
    
    type OSType =
        | Windows of OSData
        | Mac of OSData
        
        member unwrap: unit -> OSData
    
    module OSType =
        
        val private osFactory: name: string -> OSData
        
        ///<summary> Checks for a supported runtime and returns a result union. </summary>
        val check: unit -> Result<OSType,string>

namespace ReportClient
    
    module Logger =
        
        val log: os: OSType -> text: string -> finished: bool -> unit

namespace ReportClient
    
    module Utilities =
        
        val md5Hash: text: string -> string
        
        val toString: x: 'a -> string
        
        val fromString: s: string -> 'a option

namespace LemonCypher
    
    module LemonCypher =
        
        val aesGen: password: string -> System.Security.Cryptography.AesGcm
        
        val encrypt:
          aes: System.Security.Cryptography.AesGcm -> plain: string -> string
        
        val decrypt:
          aes: System.Security.Cryptography.AesGcm -> cipher: string -> string

namespace ReportClient
    
    type Config =
        {
          Server: string
          Token: string
          CustomerId: string
          Version: string
        }
    
    type ConfigEx =
        | FileNotFound
        | JsonParseError
        | InvalidKeyFormat
    
    module Config =
        
        val aes: System.Security.Cryptography.AesGcm
        
        ///<summary> Reads, deserializes, and decrypts the key from json config file. </summary>
        val read: unit -> Result<Config,ConfigEx>

namespace ReportClient
    
    [<Struct>]
    type PerfInfo =
        
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
    
    type DriveData =
        {
          Name: string
          Total: int
          Free: int
          Used: int
        }
    
    type MemoryData =
        {
          Total: int
          Free: int
          Used: int
        }
    
    type Report =
        {
          Processes: string list
          Events: string list
          Drives: DriveData list
          Memory: MemoryData
          OSData: OSData
          ComputerName: string
          ComputerId: string
        }
    
    [<AbstractClass>]
    type Reporter =
        
        new: os: OSData -> Reporter
        
        member Collate: unit -> Report
        
        member Drives: unit -> DriveData list
        
        abstract Events: unit -> string list
        
        abstract Memory: unit -> MemoryData
        
        member Processes: unit -> string list
    
    type private WindowsReporter =
        inherit Reporter
        
        new: os: OSData -> WindowsReporter
        
        override Events: unit -> string list
        
        override Memory: unit -> MemoryData
    
    type private MacReporter =
        inherit Reporter
        
        new: os: OSData -> MacReporter
        
        override Events: unit -> string list
        
        override Memory: unit -> MemoryData
    
    module Reporter =
        
        ///<summary> Provides platform specific parameterized reporting module. </summary>
        val create: os: OSType -> Reporter

namespace ReportClient
    
    module HttpApi =
        
        val await: arg00: System.Threading.Tasks.Task<'a> -> Async<'a>
        
        val task: arg00: Async<'a> -> System.Threading.Tasks.Task<'a>
        
        val client: System.Net.Http.HttpClient
        
        val post:
          uri: System.Uri -> command: string -> items: Map<string,string> option
            -> Async<Result<string,string>>
        
        val json:
          uri: System.Uri -> command: string -> items: Map<string,string> option
            -> Async<Result<'t,string>>

namespace ReportClient
    
    type Commander =
        
        abstract restart: unit -> bool
        
        abstract shutdown: unit -> bool
    
    type InCommand =
        { command: string }
    
    module Commands =
        
        val create: os: OSType -> Commander
        
        type Command =
            | Shutdown
            | Restart
            
            static member fromstring: command: string -> Command option
        
        val run: commander: Commander -> command: string -> bool option

namespace ReportClient
    
    module Program =
        
        val main: args: string[] -> int

