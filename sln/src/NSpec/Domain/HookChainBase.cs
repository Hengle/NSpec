using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public abstract class HookChainBase
    {
        public void BuildMethodLevel(List<Type> classHierarchy)
        {
            var methods = GetMethodsFromHierarchy(classHierarchy, methodSelector);

            if (reversed) 
            {
                methods.Reverse();
            }

            if (methods.Count > 0)
            {
                ClassHook = instance => methods.Do(m => m.Invoke(instance, null));
            }

            var asyncMethods = GetMethodsFromHierarchy(classHierarchy, asyncMethodSelector);

            if (reversed)
            {
                asyncMethods.Reverse();
            }

            if (asyncMethods.Count > 0)
            {
                AsyncClassHook = instance => asyncMethods.Do(m => new AsyncMethodLevelBefore(m).Run(instance));
            }
        }

        protected abstract bool CanRun(nspec instance);

        public void Run(nspec instance)
        {
            if (CanRun(instance))
            {
                RunAndHandleException(RunHooks, instance, ref Exception);
            }
        }

        protected virtual void RunHooks(nspec instance)
        {
            // class (method-level)
            RunClassHooks(instance);

            // context-level
            RunContextHooks();
        }

        public static bool RunAndHandleException(Action<nspec> action, nspec instance, ref Exception exceptionToSet)
        {
            bool hasThrown = false;

            try
            {
                action(instance);
            }
            catch (TargetInvocationException invocationException)
            {
                if (exceptionToSet == null) exceptionToSet = instance.ExceptionToReturn(invocationException.InnerException);

                hasThrown = true;
            }
            catch (Exception exception)
            {
                if (exceptionToSet == null) exceptionToSet = instance.ExceptionToReturn(exception);

                hasThrown = true;
            }

            return hasThrown;
        }

        protected void RunClassHooks(nspec instance)
        {
            // class (method-level)

            if (ClassHook != null && AsyncClassHook != null)
            {
                throw new AsyncMismatchException(
                    $"A spec class with all its ancestors cannot set both sync and async " +
                    "class-level '{classHookName}' hooks, they should either be all sync or all async");
            }

            ClassHook.SafeInvoke(instance);

            AsyncClassHook.SafeInvoke(instance);
        }

        protected void RunContextHooks()
        {
            // context-level

            if (Hook != null && AsyncHook != null)
            {
                throw new AsyncMismatchException(
                    $"A single context cannot set both a '{hookName}' and an '{asyncHookName}', please pick one of the two");
            }

            if (Hook != null && Hook.IsAsync())
            {
                throw new AsyncMismatchException(
                    $"'{hookName}' cannot be set to an async delegate, please use '{asyncHookName}' instead");
            }

            Hook.SafeInvoke();

            AsyncHook.SafeInvoke();
        }

        protected static List<MethodInfo> GetMethodsFromHierarchy(
            List<Type> classHierarchy, Func<Type, MethodInfo> selectMethod)
        {
            return classHierarchy
                .Select(selectMethod)
                .Where(m => m != null)
                .ToList();
        }

        public HookChainBase(Context context,
            string hookName, string asyncHookName, string classHookName, bool reversed = false)
        {
            this.context = context;
            this.hookName = hookName;
            this.asyncHookName = asyncHookName;
            this.classHookName = classHookName;
            this.reversed = reversed;
        }

        public Action Hook;
        public Func<Task> AsyncHook;
        
        public Action<nspec> ClassHook { get; protected set; }
        public Action<nspec> AsyncClassHook { get; protected set; }

        public Exception Exception;

        protected Func<Type, MethodInfo> methodSelector;
        protected Func<Type, MethodInfo> asyncMethodSelector;

        protected readonly Context context;
        protected readonly bool reversed;
        protected readonly string hookName;
        protected readonly string asyncHookName;
        protected readonly string classHookName;
    }

    public abstract class TraversingHookChain : HookChainBase
    {
        protected override void RunHooks(nspec instance)
        {
            // parent chain
            if (!reversed)
            {
                RecurseAncestors(c => chainSelector(c).RunHooks(instance));
            }

            base.RunHooks(instance);

            // parent chain, reverse order
            if (reversed)
            {
                RecurseAncestors(c => chainSelector(c).RunHooks(instance));
            }
        }

        protected void RecurseAncestors(Action<Context> ancestorAction)
        {
            if (context.Parent != null) ancestorAction(context.Parent);
        }

        public TraversingHookChain(Context context,
            string hookName, string asyncHookName, string classHookName, bool reversed = false)
            : base(context, hookName, asyncHookName, classHookName, reversed)
        { }

        protected Func<Context, TraversingHookChain> chainSelector;
    }
}
