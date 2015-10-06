namespace Org.Kevoree.Library

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registry:ModelRegistry) =

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            let kevoreeChannelFound = registry.lookup(c.getHub())
            let kevoreeComponentFound = registry.lookup(c.getPort().eContainer())
            false
        member this.Undo() = 
            let cmd = new RemoveBindingCommand(c, nodeName, registry)
            let _ = (cmd :>  Org.Kevoree.Core.Api.Command.ICommand).Execute()
            ()