namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type UpdateCallMethodCommand(c:IInstanceMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = false
        member this.Undo() = ()