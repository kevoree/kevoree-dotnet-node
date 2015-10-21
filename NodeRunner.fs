namespace Org.Kevoree.Library.Runner

// TODO : still useful ?

type ComponentRunner =
    inherit Org.Kevoree.Core.Api.IRunner
    abstract Run:Unit

type NodeRunner() =
    inherit System.MarshalByRefObject()
    let mutable _pluginPath = null
    interface ComponentRunner with
        member this.setPluginPath(pluginPath:string):unit =
            let _ = _pluginPath = pluginPath
            ()

        member this.Run = 
            printfn "This is an integer"
            ()
    

