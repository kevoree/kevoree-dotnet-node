namespace Org.Kevoree.Library

open Org.Kevoree.Log.Api

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registry:ModelRegistry, logger:ILogger) =
    inherit System.MarshalByRefObject()

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Executed AddBinding")
            let kevoreeChannelFound = registry.[c.getHub().CastToKFMContainer().path()]
            let kevoreeComponentFound = registry.[c.getPort().eContainer().path()]
            if kevoreeChannelFound <> null && kevoreeComponentFound <> null then
                true
            else 
                false
        member this.Undo() = 
            logger.Debug("Undo AddBinding")
            let cmd = new RemoveBindingCommand(c, nodeName, registry, logger)
            let _ = (cmd :>  Org.Kevoree.Core.Api.Command.ICommand).Execute()
            ()
        member this.Name() = sprintf "[AddBinding nodeName=%s]" nodeName