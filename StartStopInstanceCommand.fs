namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type StartStopInstanceCommand(c:IInstanceMarshalled, nodeName:string, start:bool, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = 
            let target = registry.[c.path()] :?> Org.Kevoree.Core.Api.IComponentRunner
            if target <> null then
                if start then target.Run() else target.Stop();
            else false
            
        member this.Undo() = ()
        member this.Name() = sprintf "[StartStopInstance start=%b nodeName=%s]" start nodeName