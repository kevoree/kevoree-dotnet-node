namespace Org.Kevoree.Library

open Org.Kevoree.Log.Api
open Org.Kevoree.Library.AdaptationType

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registryManager:Org.Kevoree.Library.AdaptationType.RegistryManager, nodePath:string ,logger:ILogger) =
    inherit System.MarshalByRefObject()

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Executed AddBinding")
            Org.Kevoree.Library.BindingsOperations.AddBinding c logger registryManager
        member this.Undo() = 
            logger.Debug("Undo AddBinding")
            let _ = Org.Kevoree.Library.BindingsOperations.RemoveBinding c logger registryManager
            ()
        member this.Name() = sprintf "[AddBinding nodeName=%s]" nodeName