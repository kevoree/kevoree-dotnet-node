namespace Org.Kevoree.Library

module Test =
    open NUnit.Framework
    open org.kevoree.factory;
    open Org.Kevoree.Library.AdaptationType
    open Org.Kevoree.Library.KevoreeKompareBean
    open org.kevoree;
    open java.io;
    open org.kevoree.pmodeling.api.trace;

    open Org.Kevoree.Core.Api;

    let rec prettyPrintB: List<AdaptationFS> -> Org.Kevoree.Core.Api.AdaptationType -> Unit = fun adapt tp ->
        match adapt with
        | [] -> ()
        | x :: xs -> 
            if x.Type = tp
            then
                System.Diagnostics.Debug.WriteLine ("\tNodePath : " + x.NodePath)
                prettyPrintB xs tp
            else 
                System.Diagnostics.Debug.WriteLine ("}")
                prettyPrint adapt


    and prettyPrint:List<AdaptationFS> -> Unit = fun adapt ->
        match adapt with
        | [] -> ()
        | x :: xs -> 
            let tp = x.Type
            System.Diagnostics.Debug.WriteLine ("Primitive type { " + tp.ToString())
            prettyPrintB adapt tp

            
        

    [<TestFixture>]
    type KevoreeKompareBeanTest() = 

        [<Test>]
        member this.Bootstrap() = 
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\empty.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\default.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
            //let _ =  CollectionAssert.AreEqual(expected, result)
            () 
        [<Test>]
        member this.AddOneInstance() = 
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\default.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\default_plus_one_ticker.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
       //     let _ =  CollectionAssert.AreEqual(expected, result)
            () 

        [<Test>]
        member this.UpdateDictionnary() = 
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\default_plus_one_ticker.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\default_plus_one_ticker_dictionnary_updated.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
       //     let _ =  CollectionAssert.AreEqual(expected, result)
            () 

        [<Test>]
        member this.RemoveChannel() =
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\before_remove_channel.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\after_remove_channel.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
        //    let _ =  CollectionAssert.AreEqual(expected, result)
            () 



        (*This time we remove a binding from a composant of node0 to a channel but another component of the same node still have a binding to the same channel so it has to be keep running *)
        [<Test>]
        member this.RemoveChannel2() =
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\before_remove_channel2.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\after_remove_channel2.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
       //     let _ =  CollectionAssert.AreEqual(expected, result)
            () 

        [<Test>]
        member this.RemoveGroup() =
            let factory = DefaultKevoreeFactory()
            let tmp1 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\after_remove_channel.json")).get(0) :?> ContainerRoot
            let file1 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp1)
            let tmp2 = factory.createJSONLoader().loadModelFromStream(new FileInputStream(@"C:\Users\mleduc\Documents\Visual Studio 2013\Projects\Solution1\testData\after_remove_channel_then_remove_group.json")).get(0) :?> ContainerRoot;
            let file2 = new Org.Kevoree.Core.Marshalled.ContainerRootMarshalled(tmp2)
            let dkf = new DefaultKevoreeFactory();
            let modelCompare = dkf.createModelCompare();
            let nodeName = "node0"
            let traces = modelCompare.diff(tmp1, tmp2)
            let result:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = plan file1  file2  nodeName (new Org.Kevoree.Core.TracesMarshalled(traces))
            let expected:Org.Kevoree.Core.Api.Adaptation.AdaptationModel = new Org.Kevoree.Core.Api.Adaptation.AdaptationModel();
            //expected.Add(new AdaptationPrimitive())
        //    let _ =  CollectionAssert.AreEqual(expected, result)
            () 
        