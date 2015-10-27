namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled
open Org.Kevoree.Log.Api

type RemoveBindingCommand(c:IMBindingMarshalled, nodeName:string, registryManager:Org.Kevoree.Library.AdaptationType.RegistryManager, logger:ILogger) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = 
            logger.Debug("Execute RemoveBinding")
            false
        member this.Undo() = 
            logger.Debug("Undo RemoveBinding")
            ()
        member this.Name() = sprintf "[RemoveBinding nodeName=%s]" nodeName