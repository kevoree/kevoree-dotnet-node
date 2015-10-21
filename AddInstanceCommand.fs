namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled
open Org.Kevoree.Log.Api

(*
    La version java possède un wrapper factory
    On va essayer de faire sans en profitant des paradigme fonctionels de F#
*)
type AddInstanceCommand(c:IInstanceMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService, logger:ILogger) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Execute AddInstance")
            // TODO : replace local repo by a value passed by the caller context
            // TODO : replace remote nuget repo by a value passed by the caller context
            let typeDef = c.GetTypeDefinition()
            let version = typeDef.getVersion()
            let name = typeDef.getName()
            logger.Debug("Execute AddInstance : " + name + ":" + version)
            let instance = bs.LoadSomething(name, version, c.path());
            let path = c.path()
            let _ = registry.Add(path, instance)
            true

        member this.Undo() =
            logger.Debug("Undo AddInstance")
            ()
        member this.Name() = sprintf "[AddInstance nodeName=%s]" nodeName