namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type RemoveBindingCommand(c:IMBindingMarshalled, nodeName:string, registry:ModelRegistry) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = false
        member this.Undo() = ()
        member this.Name() = sprintf "[RemoveBinding nodeName=%s]" nodeName