namespace Org.Kevoree.Library

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registry:ModelRegistry) =
    inherit System.MarshalByRefObject()

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            let kevoreeChannelFound = registry.[c.getHub().CastToKFMContainer().path()]
            let kevoreeComponentFound = registry.[c.getPort().eContainer().path()]
            if kevoreeChannelFound <> null && kevoreeComponentFound <> null then
                true
            else 
                false
        member this.Undo() = 
            let cmd = new RemoveBindingCommand(c, nodeName, registry)
            let _ = (cmd :>  Org.Kevoree.Core.Api.Command.ICommand).Execute()
            ()
        member this.Name() = sprintf "[AddBinding nodeName=%s]" nodeName