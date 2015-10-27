namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api
open System.Collections.Generic

type PortInput(name:string, path:string, comp:IComponentRunner) =
    inherit System.MarshalByRefObject() 
    
    let injector:Org.Kevoree.Library.Annotation.KevoreeInjector<Org.Kevoree.Annotation.Input> = new Org.Kevoree.Library.Annotation.KevoreeInjector<Org.Kevoree.Annotation.Input>();

    
    interface Port with
        member this.send (payload:string, callback:Callback):unit = comp.sendThroughInputPort(name, payload);
            
        member this.getPath ():string = path
        member this.getConnectedBindingsSize ():int = -1