namespace Kevoree.Api

open org.kevoree;
open Kevoree.Api.AdaptationType;
open org.kevoree.modeling.api.trace;

module KevoreeKompareBean =  

    type PlanType = ContainerRoot -> ContainerRoot -> string -> TraceSequence -> AdaptationModel
    type Context = {
        NodeName: string;
        TargetNode: ContainerNode;
        TargetModel: ContainerRoot;
        CurrentNode: ContainerNode;
        CurrentModel:ContainerRoot;
        ModelRegistry: ModelRegistry
    }
    type TraceToAdaptation = ModelTrace -> Context -> AdaptationModel

    let traceToAdaptationComponent: TraceToAdaptation = fun trace context -> 
        if context.TargetNode.path() = trace.getSrcPath() 
        then
            match trace with
            | :? ModelAddTrace -> set [ { 
                                            Type =  AdaptationType.AddInstance; 
                                            NodePath = context.TargetNode.path()
                                            Ref = context.TargetModel.findByPath((trace :?>  ModelAddTrace).getPreviousPath()) } 
                                        ]
            | :? ModelRemoveTrace ->  
                let removedObjetPath = (trace :?> ModelRemoveTrace).getObjPath()
                set [ { 
                        Type =  AdaptationType.RemoveInstance; 
                        NodePath = removedObjetPath
                        Ref = context.TargetModel.findByPath(removedObjetPath) };
                        { 
                        Type =  AdaptationType.StopInstance; 
                        NodePath = removedObjetPath
                        Ref = context.TargetModel.findByPath(removedObjetPath) } 
                    ]
            | _ -> set []
        else set []

    // TODO : ici différent entre js et java, voir quoi faire, surement en rapport avec deployUnits
    let traceToAdaptationDeployUnit: TraceToAdaptation = fun _ _ -> set [] // failwith "TODO traceToAdaptationDeployUnit"
    let traceToAdaptationBindings: TraceToAdaptation = fun trace context -> 
        if not (context.TargetModel.findByPath(trace.getSrcPath()) :? Channel)
        then
            let nodePath = (trace :?> ModelAddTrace).getPreviousPath()
            let binding = context.TargetModel.findByPath(nodePath) :?> MBinding
            let channel = binding.getHub()
            
            let concatMe1 = set [ { 
                                    Type =  AdaptationType.AddBinding; 
                                    NodePath = nodePath // TODO : check que c'est bien le bon chemin
                                    Ref = binding } ]
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
        if modelElement :? Instance &&  trace :? ModelSetTrace 
        then
            if modelElement.eContainer() :? ContainerNode && not (modelElement.eContainer().path() = context.TargetNode.path())
            then set []
            else
                if  trace.getSrcPath() = context.TargetNode.path()
                then set []
                else 
                    if (trace :?> ModelSetTrace).getContent().ToLower() = "true"
                    then set [{ 
                                Type =  AdaptationType.StartInstance;
                                NodePath = context.TargetNode.path()
                                Ref = modelElement }]
                    else set [{ 
                                Type =  AdaptationType.StopInstance; 
                                NodePath = context.TargetNode.path()
                                Ref = modelElement }]
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
        if modelElement :? Value && modelElement.getRefInParent() = "values"
        then 
            let parentInstance = modelElement.eContainer().eContainer() :?> Instance
            if parentInstance <> null 
                && parentInstance :? ContainerNode 
                && parentInstance.getName() = context.NodeName 
                && context.CurrentNode = null
            then set []
            else 
                let dictionaryParent = modelElement.eContainer()
                if dictionaryParent <> null 
                    && dictionaryParent :? FragmentDictionary 
                    && (dictionaryParent :?> FragmentDictionary).getName() <> context.NodeName
                then set []
                else 
                    let set1 = set [{
                                        Type = AdaptationType.UpdateDictionaryInstance;
                                        NodePath = context.TargetNode.path(); 
                                        Ref = [ modelElement; modelElement.eContainer().eContainer() ] }]
                    let set2 = 
                        if parentInstance <> null
                        then set [ { Type = AdaptationType.UpdateCallMethod; NodePath = parentInstance.path(); Ref = parentInstance } ]
                        else set []

                    set1 + set2
        else  set []

    let traceToAdaptation : Context -> (AdaptationModel -> ModelTrace -> AdaptationModel) = fun context adaptations trace ->
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


    let plan:ContainerRoot -> ContainerRoot -> string -> TraceSequence -> AdaptationModel = fun current target nodeName traces -> 
        // TODO : cleanup cet horreur !!!
        let afd = new System.Collections.Generic.List<ModelTrace>();
        let aa = (traces.getTraces().iterator())
        while (aa.hasNext()) do
            let tmp = (aa.next())
            afd.Add(tmp :?> ModelTrace)

        let asdf:list<ModelTrace> = afd.ToArray() |> Array.toList
        
        let context = {
            NodeName = nodeName;
            TargetNode = target.findNodesByID(nodeName);
            TargetModel = target;
            CurrentNode = current.findNodesByID(nodeName);
            CurrentModel = current;
            ModelRegistry = Map.empty }
        List.fold (traceToAdaptation context) Set.empty asdf