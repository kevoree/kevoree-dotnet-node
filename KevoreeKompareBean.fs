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

    let rec isRelatedToPlatform(element: IKMFContainerMarshalled, context:Context):bool = 
        if element.isOfType(typedefof<ComponentInstance>) then
            let a = element.CastToComponentInstance()
            let cn = a.eContainer().CastToContainerNode()
            let currn = context.CurrentNode
            if a.eContainer().isOfType(typedefof<ContainerNode>) then cn.getName() = currn.getName() else false
        elif element.isOfType(typedefof<Channel>) then
            element.CastToChannel().getBindings().Exists (
                fun binding ->  (binding.getPort() <> null) && (binding.getPort().eContainer() <> null) && (isRelatedToPlatform(binding.getPort().eContainer().CastToKFMContainer(), context))
            )
            //TODO : continuer à implanter cette partie du comparateur en se basant sur https://github.com/kevoree/kevoree-js-node-javascript/blob/eb8abc7d20e7e52a87a86301e4af50e5b62d63fc/lib/AdaptationEngine.js
        elif element.isOfType(typedefof<Group>) then element.CastToGroup().getName() = context.CurrentNode.getName()
        elif element.isOfType(typedefof<ContainerNode>) then 
            let containerNodez:IContainerNodeMarshalled = element.CastToContainerNode()
            ((containerNodez.getName() = context.CurrentNode.getName()) || (containerNodez.getHost() <> null && containerNodez.getHost().getName() = context.CurrentNode.getName()))
        elif element.isOfType(typedefof<MBinding>) then
            let mbinding = element.CastToMBinding()
            (mbinding.getPort() <> null && mbinding.getPort().eContainer() <> null && isRelatedToPlatform(mbinding.getPort().eContainer(), context)) || (mbinding.getHub() <> null && isRelatedToPlatform(mbinding.getHub().CastToKFMContainer(), context))
        elif element.isOfType(typedefof<Value>) then
            let value = element.CastToValue()
            (value.eContainer().isOfType(typedefof<FragmentDictionary>) && value.eContainer().CastToFragmentDictionary().getName() = context.CurrentNode.getName()) || isRelatedToPlatform(element.eContainer().eContainer(), context)
        elif element.isOfType(typedefof<Port>) then isRelatedToPlatform(element.eContainer(), context)
        else false

    let traceToAdaptationComponent: TraceToAdaptation = fun trace context -> 
        if context.TargetNode.path() = trace.getSrcPath() 
        then
            if trace.isOfType(typedefof<ModelAddTrace>) then
                let ref = context.TargetModel.findByPath(trace.getModelAddTrace().getPreviousPath())
                set [ {  Type =  AdaptationType.AddInstance;  NodePath = context.TargetNode.path(); Ref = ref; Ref2 = None }  ]
            elif trace.isOfType(typedefof<ModelRemoveTrace>) then
                let removedObjetPath = trace.getModelRemoveTrace().getObjPath()
                set [ { Type =  AdaptationType.RemoveInstance;  NodePath = removedObjetPath; Ref = context.CurrentModel.findByPath(removedObjetPath); Ref2 = None }; {  Type =  AdaptationType.StopInstance;  NodePath = removedObjetPath; Ref = context.CurrentModel.findByPath(removedObjetPath); Ref2 = None }  ]
            else set []
        else set []

    // TODO : ici différent entre js et java, voir quoi faire, surement en rapport avec deployUnits
    let traceToAdaptationDeployUnit: TraceToAdaptation = fun trace context ->
        let modelElement = context.TargetModel.findByPath(trace.getSrcPath())
        if trace.getTraceType().name() = "ADD" && isRelatedToPlatform(modelElement, context)
        then set [{ Type = AdaptationType.AddDeployUnit; NodePath = ""; Ref = modelElement; Ref2 = None }]
        elif trace .getTraceType().name() = "REMOVE"
        then
            let removedObjetPath = trace.getModelRemoveTrace().getObjPath()
            let du = context.TargetModel.findByPath(removedObjetPath);
            if du <> null  then set [{ Type = AdaptationType.RemoveDeployUnit; NodePath = ""; Ref = du; Ref2 = None }] else set []
        else set []

    let traceToAdaptationBindings: TraceToAdaptation = fun trace context ->
        if not (context.TargetModel.findByPath(trace.getSrcPath()).isOfType(typedefof<Channel>)) then 
            if trace.isOfType(typedefof<ModelAddTrace>) then
                let binding = context.TargetModel.findByPath(trace.getModelAddTrace().getPreviousPath()).CastToMBinding()
                let addBinding = set[ { Type = AdaptationType.AddBinding; NodePath=""; Ref=binding.CastToKFMContainer(); Ref2=None}]
                let channel = binding.getHub()
                (* if we have a bind we check if our local registry already contains a simmilar node and if it does not, we add it *)
                let addInstance =   if (channel <> null && isRelatedToPlatform(binding.CastToKFMContainer(), context) && not (context.ModelRegistry.ContainsKey(channel.path())))
                                    then 
                                        let ref = channel.CastToKFMContainer()
                                        let addInstanceInner = set [{Type=AdaptationType.AddInstance; NodePath=""; Ref=ref; Ref2=None }]
                                        let updateDictionary =  if channel.getDictionary() <> null
                                                                then List.map (fun (value:IValueMarshalled) -> { Type=AdaptationType.UpdateDictionary; NodePath=""; Ref=channel.CastToKFMContainer(); Ref2=Some(value.CastToKFMContainer()) }) (List.ofArray (channel.getDictionary().getValues().ToArray()))  |> Set.ofList
                                                                else set []
                                        let updateFragmentDictionarty:Set<AdaptationFS> = List.concat (List.map (fun (value:IFragmentDictionaryMarshalled) -> if value.getName() = context.NodeName then List.map (fun (v:IValueMarshalled) -> { Type=AdaptationType.UpdateDictionary; NodePath=""; Ref=channel.CastToKFMContainer(); Ref2=Some(v.CastToKFMContainer()) }) (List.ofArray (value.getValues().ToArray())) else []) (List.ofArray (channel.getFragmentDictionary().ToArray()))) |> Set.ofList
                                        addInstanceInner + updateDictionary + updateFragmentDictionarty
                                    else set []
                let startInstance = if channel.getStarted() then set [{Type=AdaptationType.StartInstance; NodePath=""; Ref=channel.CastToKFMContainer(); Ref2=None}] else set []
                addBinding + addInstance + startInstance
                
            elif trace.isOfType(typedefof<ModelRemoveTrace>) then 
                let binding = context.CurrentModel.findByPath(trace.getModelRemoveTrace().getObjPath()).CastToMBinding()
                let hubPath = binding.getHub().path()
                let kmfChannel = context.CurrentModel.findByPath(hubPath)
                if kmfChannel <> null then
                    let kmfNewChan = context.TargetModel.findByPath(hubPath)
                    (* We check if the channel still exists in the new channel. 
                    If it does not, it can be removed.
                    If it does, we check if it still has relationships with our node. It it does not we can safely remove the framgment on the channel in our node. *)
                    let existBinding =  if kmfNewChan <> null then List.exists (fun (a:IMBindingMarshalled) -> isRelatedToPlatform(a.CastToKFMContainer(),context)) (List.ofArray (kmfNewChan.CastToChannel().getBindings().ToArray())) else false
                    let oldChannel = context.CurrentModel.findByPath(binding.getHub().path())
                    let removeInstance  = if (not existBinding) && context.ModelRegistry.ContainsKey(oldChannel.path()) then set [{Type=AdaptationType.RemoveInstance; NodePath=""; Ref=binding.getHub().CastToKFMContainer(); Ref2=None}] else set []
                    let stopChannel = if (not existBinding) && binding.getHub().getStarted() then set [{Type=AdaptationType.StopInstance; NodePath=""; Ref=oldChannel.CastToKFMContainer(); Ref2=None}] else set []
                    let removeBinding = if isRelatedToPlatform(binding.CastToKFMContainer(), context) then set [{Type=AdaptationType.RemoveBinding; NodePath=""; Ref=binding.CastToKFMContainer(); Ref2=None}] else set []
                    removeBinding + stopChannel + removeInstance
                else set []
            else set []
        else set []


    let traceToAdaptationStarted: TraceToAdaptation = fun trace context ->
        let srcPath:string = trace.getSrcPath()
        let modelElement:IKMFContainerMarshalled = context.TargetModel.findByPath(srcPath)
        if modelElement.isOfType(typedefof<Instance>) && trace.isOfType(typedefof<ModelSetTrace>)
        then
            //let instanceElement = modelElement.CastToInstance()
            if modelElement.eContainer().isOfType(typedefof<ContainerNode>) && not (modelElement.eContainer().path() = context.TargetNode.path())
            then set []
            else
                if  trace.getSrcPath() = context.TargetNode.path()
                then set []
                else 
                    if trace.getModelSetTrace().getContent().ToLower() = "true"
                    then 
                        let start = set [{ Type =  AdaptationType.StartInstance; NodePath = context.TargetNode.path(); Ref = modelElement; Ref2 = None }]
                        (*let kDict = instanceElement.getDictionary();
                        let values = if kDict <> null then kDict.getValues() else null
                        let dicti = if values <> null
                                    then Array.map (fun (e:IValueMarshalled) -> {Type = AdaptationType.UpdateDictionary; NodePath = context.TargetNode.path(); Ref = instanceElement.CastToKFMContainer(); Ref2 = Some(e.CastToKFMContainer()) }) (values.ToArray()) |> Set.ofArray
                                    else set []
                        let fDict = instanceElement.getFragmentDictionary();
                        let dictj = if values <> null
                                    then Array.map (fun (e:IValueMarshalled) -> {Type = AdaptationType.UpdateDictionary; NodePath = context.TargetNode.path(); Ref = instanceElement.CastToKFMContainer(); Ref2 = Some(e.CastToKFMContainer()) }) (values.ToArray()) |> Set.ofArray
                                    else set []
                        // TODO : peut être utile ici de filtrer les mises à jours inutiles, c'est à dire les update de valeurs qui n'ont pas changées dans le temps.*)
                        start // + dicti
                    else set [{ 
                                Type = AdaptationType.StopInstance; 
                                NodePath = context.TargetNode.path()
                                Ref = modelElement
                                Ref2 = None }]
        else set []
    let traceToAdaptationIgnored:TraceToAdaptation = fun trace context -> set []

    let traceToAdaptationTypeValue:TraceToAdaptation = fun trace context ->
        let traceSrcPath = trace.getSrcPath()
        let modelElement = context.TargetModel.findByPath(traceSrcPath)
        let c1 = modelElement.isOfType(typedefof<Value>);
        let refInParent = modelElement.getRefInParent()
        let c2 = refInParent = "values"
        if c1 && c2
        then 
            let parentInstance = modelElement.eContainer().eContainer().CastToInstance()
            if parentInstance <> null
                && parentInstance.isOfType(typedefof<ContainerNode>)
                && parentInstance.getName() = context.NodeName 
                && context.CurrentNode = null
            then set []
            else 
                let dictionaryParent = modelElement.eContainer()
                
                if (dictionaryParent <> null
                    && dictionaryParent.isOfType(typedefof<FragmentDictionary>)
                    && dictionaryParent.CastToFragmentDictionary().getName() <> context.NodeName) 
                then set []
                else 
                    // TODO : a quoi sers le path ?
                    let set1 = set [{ Type = AdaptationType.UpdateDictionary; NodePath=""; Ref=modelElement.eContainer().eContainer(); Ref2=Some(modelElement)}] 
                    let set2 = if dictionaryParent = null then set [] else set [ { Type = AdaptationType.UpdateInstance; NodePath=""; Ref=dictionaryParent; Ref2=None}] 
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
            let _ = am.Add(convertAdapt elem)
            ()
        am


    let plan:IContainerRootMarshalled -> IContainerRootMarshalled -> string -> Org.Kevoree.Core.Api.ITracesSequence -> Org.Kevoree.Library.AdaptationType.ModelRegistry -> Org.Kevoree.Core.Api.Adaptation.AdaptationModel = fun current target nodeName traces registry ->         
        let asdf = traces.GetTraces() |> List.ofSeq;
        let context = {
            NodeName = nodeName;
            TargetNode = target.findNodesByID(nodeName);
            TargetModel = target;
            CurrentNode = if current.findNodesByID(nodeName) <> null then current.findNodesByID(nodeName) else target.findNodesByID(nodeName);
            CurrentModel = current;
            ModelRegistry = registry }
        let result:AdaptationModelFS = List.fold (traceToAdaptation context) Set.empty asdf
        convert result