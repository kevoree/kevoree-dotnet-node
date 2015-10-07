namespace Org.Kevoree.Library

open org.kevoree
open org.kevoree.pmodeling.api.trace
open Org.Kevoree.Core.Api.Adaptation
open Org.Kevoree.Library.AdaptationType
open Org.Kevoree.Core.Api
open Org.Kevoree.Core.Api.IMarshalled

module KevoreeKompareBean =

    type PlanType = ContainerRoot -> ContainerRoot -> string -> TraceSequence -> AdaptationModel
    type Context = {
        NodeName: string;
        TargetNode: IContainerNodeMarshalled;
        TargetModel: IContainerRootMarshalled;
        CurrentNode: IContainerNodeMarshalled;
        CurrentModel: IContainerRootMarshalled;
        ModelRegistry: ModelRegistry
    }
    type TraceToAdaptation = Org.Kevoree.Core.Api.IModelTraceMarshalled -> Context -> AdaptationModelFS

    let traceToAdaptationComponent: TraceToAdaptation = fun trace context -> 
        if context.TargetNode.path() = trace.getSrcPath() 
        then
            if trace.isOfType(typedefof<ModelAddTrace>) then set [ {  Type =  AdaptationType.AddInstance;  NodePath = context.TargetNode.path(); Ref = context.TargetModel.findByPath(trace.getModelAddTrace().getPreviousPath()); Ref2 = None }  ]
            elif trace.isOfType(typedefof<ModelRemoveTrace>) then
                let removedObjetPath = trace.getModelRemoveTrace().getObjPath()
                set [ { Type =  AdaptationType.RemoveInstance;  NodePath = removedObjetPath; Ref = context.TargetModel.findByPath(removedObjetPath); Ref2 = None }; {  Type =  AdaptationType.StopInstance;  NodePath = removedObjetPath; Ref = context.TargetModel.findByPath(removedObjetPath); Ref2 = None }  ]
            else set []
        else set []

    // TODO : ici différent entre js et java, voir quoi faire, surement en rapport avec deployUnits
    let traceToAdaptationDeployUnit: TraceToAdaptation = fun _ _ -> set [] // failwith "TODO traceToAdaptationDeployUnit"
    let traceToAdaptationBindings: TraceToAdaptation = fun trace context ->
        if not (context.TargetModel.findByPath(trace.getSrcPath()).isOfType(typedefof<Channel>))
        then
            let nodePath = trace.getModelAddTrace().getPreviousPath()
            let binding = context.TargetModel.findByPath(nodePath).CastToMBinding()
            let channel = binding.getHub()
            
            let concatMe1 = set [ { 
                                    Type =  AdaptationType.AddBinding; 
                                    NodePath = nodePath // TODO : check que c'est bien le bon chemin
                                    Ref = binding.CastToKFMContainer()
                                    Ref2 = None } ]
            let concatMe2 = 
                if channel <> null //&& context.ModelRegistry.lookup(channel) = null
                then set []
                else set []
            concatMe1 + concatMe2
            (*if trace :? ModelAddTrace
            then 
                let pt1 = [ { 
                            Type =  AdaptationType.AddInstance; 
                            NodePath = (trace :?> ModelAddTrace).getPreviousPath() // TODO : check que c'est bien le bon chemin
                            Ref = context.TargetModel.findByPath(removedObjetPath) } ]
            else set []*)
        else set []


    let traceToAdaptationStarted: TraceToAdaptation = fun trace context ->
        let modelElement = context.TargetModel.findByPath(trace.getSrcPath())
        if modelElement.isOfType(typedefof<Instance>) && trace.isOfType(typedefof<ModelSetTrace>)
        then
            if modelElement.eContainer().isOfType(typedefof<ContainerNode>) && not (modelElement.eContainer().path() = context.TargetNode.path())
            then set []
            else
                if  trace.getSrcPath() = context.TargetNode.path()
                then set []
                else 
                    if trace.getModelSetTrace().getContent().ToLower() = "true"
                    then set [{ 
                                Type =  AdaptationType.StartInstance;
                                NodePath = context.TargetNode.path()
                                Ref = modelElement
                                Ref2 = None }]
                    else set [{ 
                                Type = AdaptationType.StopInstance; 
                                NodePath = context.TargetNode.path()
                                Ref = modelElement
                                Ref2 = None }]
        else set []
    let traceToAdaptationTypeDefinition: TraceToAdaptation = fun trace context -> set []
        // TODO : a implémenter pour gérer le changement de version d'un élémént @Runtime
        (*let modelElement = context.TargetModel.findByPath(trace.getSrcPath())
        if trace :? ModelAddTrace && modelElement :? Instance
        then
            let currentModelElement = context.CurrentModel.findByPath(modelElement.path()) :?> Instance
            let targetModelElement = context.TargetModel.findByPath(modelElement.path()) :?> Instance
            if currentModelElement <> null && targetModelElement <> null
            then
                if modelElement.path() = context.TargetNode.path()
                then set []
                else
                    if currentModelElement.getStarted() && targetModelElement.getStarted()
                    then set [] // TODO
                    else
                        if currentModelElement :?  Channel
                        then set [] // TODO
                        else 
            else set []
        else set []*)
    let traceToAdaptationIgnored:TraceToAdaptation = fun trace context -> set []

    let traceToAdaptationTypeValue:TraceToAdaptation = fun trace context ->
        let modelElement = context.TargetModel.findByPath(trace.getSrcPath())
        if modelElement.isOfType(typedefof<Value>) && modelElement.getRefInParent() = "values"
        then 
            let parentInstance = modelElement.eContainer().eContainer().CastToInstance()
            if parentInstance <> null
                && parentInstance.isOfType(typedefof<ContainerNode>)
                && parentInstance.getName() = context.NodeName 
                && context.CurrentNode = null
            then set []
            else 
                let dictionaryParent = modelElement.eContainer()
                if dictionaryParent <> null
                    && dictionaryParent.isOfType(typedefof<FragmentDictionary>)
                    && dictionaryParent.CastToFragmentDictionary().getName() <> context.NodeName
                then set []
                else 
                    let set1 = set [{
                                        Type = AdaptationType.UpdateDictionaryInstance
                                        NodePath = context.TargetNode.path()
                                        Ref = modelElement
                                        Ref2 = Some(modelElement.eContainer().eContainer()) }]
                    let set2 = 
                        if parentInstance <> null
                        then set [ { Type = AdaptationType.UpdateCallMethod; NodePath = parentInstance.path(); Ref = parentInstance.CastToKFMContainer(); Ref2 = None } ]
                        else set []

                    set1 + set2
        else  set []

    let traceToAdaptation : Context -> (AdaptationModelFS -> Org.Kevoree.Core.Api.IModelTraceMarshalled -> AdaptationModelFS) = fun context adaptations trace ->
        let adt = match (trace.getRefName()) with
                    | "groups" ->  traceToAdaptationComponent trace context
                    | "hosts" -> traceToAdaptationComponent trace context
                    | "components" -> traceToAdaptationComponent trace context
                    | "deployUnits" -> traceToAdaptationDeployUnit trace context
                    | "bindings" -> traceToAdaptationBindings trace context
                    | "started" -> traceToAdaptationStarted trace context
                    | "typeDefinition" -> traceToAdaptationTypeDefinition trace context
                    | "value" -> traceToAdaptationTypeValue trace context
                    | _ -> traceToAdaptationIgnored trace context // TODO : ajout des logs sur 
        
        adaptations + adt

    let convertAdapt:AdaptationFS -> AdaptationPrimitive = fun adaptationFS ->
        let ret = new AdaptationPrimitive()
        ret.setType(adaptationFS.Type)
        ret.setNodePath(adaptationFS.NodePath)
        ret.setRef(adaptationFS.Ref)
        let ref2 = match adaptationFS.Ref2 with
            | Some(x) -> x
            | None -> null
        ret.setRef2(ref2)
        ret

    let convert:AdaptationModelFS -> Org.Kevoree.Core.Api.Adaptation.AdaptationModel = fun adaptationModelfs ->
        let am = new AdaptationModel();
        for elem in adaptationModelfs do
            am.Add(convertAdapt elem)
        am


    let plan:IContainerRootMarshalled -> IContainerRootMarshalled -> string -> Org.Kevoree.Core.Api.ITracesSequence -> Org.Kevoree.Core.Api.Adaptation.AdaptationModel = fun current target nodeName traces ->         
        let asdf = traces.GetTraces() |> List.ofSeq;
        let context = {
            NodeName = nodeName;
            TargetNode = target.findNodesByID(nodeName);
            TargetModel = target;
            CurrentNode = current.findNodesByID(nodeName);
            CurrentModel = current;
            ModelRegistry = Map.empty }
        convert (List.fold (traceToAdaptation context) Set.empty asdf)