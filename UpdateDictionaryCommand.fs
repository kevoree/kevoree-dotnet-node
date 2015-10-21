namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled

type UpdateDictionaryCommand(c:IInstanceMarshalled, dicValue:IValueMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            (*let previousModel =  modelService.getCurrentModel().getModel()
            let previousValue = previousModel.findByPath(dicValue.path());*)
            let path = c.path()
            let lookup = registry.ContainsKey(path)
            if lookup then
                let component = registry.[path] :?> Org.Kevoree.Core.Api.IComponentRunner
                let attributeDefinition = dicValue.eContainer().eContainer().CastToInstance().GetTypeDefinition().getDictionaryType().findAttributesByID(dicValue.getName());
                component.updateDictionary(attributeDefinition, dicValue);
                true
            else false
        member this.Undo() = ()
        member this.Name() = sprintf "[UpdateDictionnary nodeName=%s]" nodeName