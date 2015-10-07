namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type UpdateDictionaryCommand(c:IInstanceMarshalled, dicValue:IValueMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() = false
        member this.Undo() = ()