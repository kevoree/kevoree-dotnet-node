namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

(*
    La version java possède un wrapper factory
    On va essayer de faire sans en profitant des paradigme fonctionels de F#
*)
type AddInstanceCommand(c:IInstanceMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            // TODO : replace local repo by a value passed by the caller context
            // TODO : replace remote nuget repo by a value passed by the caller context
            let typeDef = c.GetTypeDefinition()
            let version = typeDef.getVersion()
            let name = typeDef.getName()
            bs.LoadSomething(name, version, c.path()).Run();
        member this.Undo() = ()
        member this.Name() = sprintf "[AddInstance nodeName=%s]" nodeName