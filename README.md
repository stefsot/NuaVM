# NuaVM
## NuaVM is a Lua 5.2 runtime and interpreter written in C# with no external or native dependencies. It can be used in both NET Core and NET framework versions to run raw lua code and bridge the lua and net world.

NuaVM currently only suppports execution of compiled Lua 5.2 bytecode and does NOT include a built-in lua compiler. You can download and use the official lua compiler (luac) from
https://www.lua.org/ftp/ (make sure to download version 5.2). I plan on adding a compiler sometime in the future.

This is a work-in-progress project that currently supports all the default lua features with minimal api. 
The initial concept of this project was to allow low level access to the internal workings of the lua virtual machine for debugging and deobfuscating purposes but it has evolved to bind the lua and net world into a simple API.

Any contributors would be greatly appreciated.

### _usage example_
```csharp
        static void Example()
        {
            // set up a default 'environment' lua table to load our scripts into
            // an 'environment' is the top level "upvalue" that the bytecode will access for its globals
            // lua code example: log = function(...) print(...) end; 
            // the 'log' global function is stored inside the 'environment' table
            var env = new NuaTable();

            // create our vm
            // currently the vm has very limited customization options
            // it mainly allows for hooks and events to monitor and modify the lua execution
            var vm = new NuaVirtualMachine();

            // register some .net functions into the 'environment' table to access from lua
            // the "context" parameter allows access to the current execution context
            // "args" are the arguments the function was invoked with
            env.Set("print", new NuaFunction((context, args) =>
            {
                foreach (var a in args)
                {
                    switch (a.Type)
                    {
                        case NuaObjectType.function:
                        case NuaObjectType.nil:
                            Console.WriteLine(a.Type);
                            break;

                        case NuaObjectType.boolean:
                        case NuaObjectType.number:
                        case NuaObjectType.@string:
                        case NuaObjectType.table:
                            Console.WriteLine(a.Value.ToString());
                            break;

                        case NuaObjectType.userdata:
                            Console.WriteLine($"userdata ({a.Value.GetType().Name}): {a.Value}");
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // return values
                return NuaObject.EmptyArgs;
            }));

            // NuaVM allows the use of "userdata" as specified by lua
            // "userdata" are custom types that can be provided to lua
            // the default userdata implementation is very simple
            // but you can implement your own advanced api by inheriting the "NuaUserData<T>" type
            env.Set("stopwatch", NuaUserData.Create(Stopwatch.StartNew()));

            // load our lua bytecode into the vm
            var byteCode = LuaCompiler.Compile(
                @"
                    elapsedSeconds = function() return stopwatch.ElapsedMilliseconds / 1000 end
                ");
            var luaMainFunction = vm.Load(new LuaAssembly(byteCode), env);

            // the LuaCompiler class is not provided into the project
            // its just a wrapper around the "luac" compiler provided by https://www.lua.org/
            // download binaries at: https://www.lua.org/ftp/lua-5.2.4.tar.gz

            // invoke the loaded lua code
            luaMainFunction.Invoke();

            // lets try to retrieve the lua function ellapsedSeconds and the net function print
            // from our env table

            var elapsedSeconds = env.Get(null, "elapsedSeconds").AsFunction();
            var print = env.Get(null, "print").AsFunction();

            // invoke
            print.Invoke(elapsedSeconds.Invoke());
        }
...
```

![alt text](https://i.imgur.com/qWZw4Jf.gif)
