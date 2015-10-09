namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type AddDeployUnitCommand(c:IDeployUnitMarshalled, bs:Org.Kevoree.Core.Api.BootstrapService)  =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = false
        member this.Undo() = ()
        member this.Name() = sprintf "[AddDeployUnit]"