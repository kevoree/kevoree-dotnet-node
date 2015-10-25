namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api
open System.Collections.Generic

type PortInput(name:string, path:string, channel:IComponentRunner) =
    inherit System.MarshalByRefObject() 

    let mutable components:Dictionary<IComponentRunner,string> = new Dictionary<IComponentRunner, string>();
    
    let injector:Org.Kevoree.Library.Annotation.KevoreeInjector<Org.Kevoree.Annotation.Input> = new Org.Kevoree.Library.Annotation.KevoreeInjector<Org.Kevoree.Annotation.Input>();

    member this.registerComponent(comp:IComponentRunner, fieldName:string):unit = components.Add(comp, fieldName)    
    
    interface Port with
        member this.send (payload:string, callback:Callback):unit =
            for entry in components do
                let componentz = entry.Key
                let methodName = entry.Value
                injector.callByName(componentz, methodName, payload)
                ()
        member this.getPath ():string = path
        member this.getConnectedBindingsSize ():int = -1