namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled
open Org.Kevoree.Log.Api

(*
    La version java possède un wrapper factory
    On va essayer de faire sans en profitant des paradigme fonctionels de F#
*)
type AddInstanceCommand(c:IInstanceMarshalled, nodeName:string, registryManager:Org.Kevoree.Library.AdaptationType.RegistryManager, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService, logger:ILogger) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Execute AddInstance")
            let path = c.path()
            if not (registryManager.Lookup path) then 
                let a = c.GetTypeDefinition().getDeployUnits().FindAll(fun x -> x.findFiltersByID("platform").getValue() = "dotnet")
                a.Sort(fun a b -> Semver.SemVersion.Parse(a.getVersion()).CompareTo(Semver.SemVersion.Parse(b.getVersion())))
                a.Reverse()
                let du:IDeployUnitMarshalled = a.[0]
                logger.Debug(du.getVersion() + " selected")
                let version = du.getVersion()
                let name = du.getName()
                logger.Debug("Execute AddInstance : " + name + ":" + version)
                let instance = bs.LoadSomething(name, version, c.path());
                registryManager.SaveToModel(path, instance)
            true

        member this.Undo() =
            logger.Debug("Undo AddInstance")
            ()
        member this.Name() = sprintf "[AddInstance nodeName=%s]" nodeName