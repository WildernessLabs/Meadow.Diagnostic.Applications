using Meadow;
using Meadow.Devices;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LinkerRepro;

public class FeatherApp : MeadowApp<F7FeatherV2> { }
public class CcmApp : MeadowApp<F7CoreComputeV2> { }

public class MeadowApp<T> : App<T>
    where T : F7MicroBase
{
    public override async Task Run()
    {
        Resolver.Log.Info("Run...");

        await RunTest();
    }

    public Task<bool> RunTest()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Validation.Utility.dat");
            Resolver.Log.Info($"Loading assembly '{path}'...");
            var assembly = Assembly.LoadFrom(path);

            if (assembly == null)
            {
                Resolver.Log.Error("Validation.Utility.dll assembly is null");
                return Task.FromResult(false);
            }

            Module? m = null;

            var modules = assembly.GetModules();
            Resolver.Log.Info($"Validation.Utility.dll contains {modules.Count()} modules:");
            foreach (var module in modules)
            {
                Resolver.Log.Info($"    {module.Name}");

                if (module.Name == "Validation.Utility.dat")
                {
                    m = module;
                }
            }

            if (m == null)
            {
                Resolver.Log.Error("Did not find module 'Validation.Utility.dat'");
                return Task.FromResult(false);
            }

            var type = m.GetType("Validation.Utility.TestClass", true);
            if (type == null)
            {
                Resolver.Log.Error("Failed to load type 'TestClass'");
                return Task.FromResult(false);
            }

            var testClass = Activator.CreateInstance(type);
            if (testClass == null)
            {
                Resolver.Log.Error("Failed to create instance of 'TestClass'");
                return Task.FromResult(false);
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            MethodInfo? add = null;
            Resolver.Log.Info($"TestClass type contains {methods.Count()} methods:");
            foreach (var method in methods)
            {
                Resolver.Log.Info($"    {method.Name}");
                if (method.Name == "Add")
                {
                    add = method;
                }
            }

            if (add == null)
            {
                Resolver.Log.Error("Failed to find 'Add' method in 'TestClass'");
                return Task.FromResult(false);
            }

            var result = add.Invoke(testClass, new object[] { 5, 7 });
            var numeric = (int)result;

            if (numeric != 7 + 5)
            {
                Resolver.Log.Error("Add returned an unexpected result");
                return Task.FromResult(false);
            }

            Resolver.Log.Info($"Reflection called `Add` succeeded");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Test failed: {ex.Message} ({ex.GetType().Name})");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}