namespace Org.Kevoree.Library

open org.kevoree.pmodeling.api
open System.Collections.Generic
open Org.Kevoree.Core.Api.IMarshalled


type ModelRegistry() =
    member x.Registry:Dictionary<string, obj> = new Dictionary<string, obj>();

    member x.lookup(elem:IKMFContainerMarshalled):obj = x.Registry.[elem.path()]

    member x.registerByContainer(elem: KMFContainer, value:obj):unit = x.Registry.Add(elem.path(), value)

    member x.registerByPath(path:string, value:obj) = x.Registry.Add(path, value)

    member x.drop(elem:KMFContainer):bool = x.Registry.Remove(elem.path())

    member x.clear():unit = x.Registry.Clear()

    (*public Object lookup(KMFContainer elem) {
        return registry.get(elem.path());
    }

    public void register(KMFContainer elem, Object obj) {
        registry.put(elem.path(), obj);
    }

    public void registerFromPath(String path, Object obj) {
        registry.put(path, obj);
    }

    public void drop(KMFContainer elem) {
        registry.remove(elem.path());
    }

    public void clear() {
        registry.clear();
    }

    public static final ThreadLocal<ModelRegistry> current = new ThreadLocal<ModelRegistry>();*)