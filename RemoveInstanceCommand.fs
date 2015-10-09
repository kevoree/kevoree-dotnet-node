namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

(*
    La version java possède un wrapper factory
    On va essayer de faire sans en profitant des paradigme fonctionels de F#
*)
type RemoveInstanceCommand(c:IInstanceMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = false
        member this.Undo() = ()
        member this.Name() = sprintf "[RemoveInstance nodeName=%s]" nodeName