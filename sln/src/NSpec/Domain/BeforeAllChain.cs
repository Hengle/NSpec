using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public class BeforeAllChain : HookChainBase
    {
        public override void BuildMethodLevel(Conventions conventions, List<Type> classHierarchy)
        {
            var methods = GetMethodsFromHierarchy(
                classHierarchy, conventions.GetMethodLevelBeforeAll);
            
            if (methods.Count > 0)
            {
                ClassHook = instance => methods.Do(m => m.Invoke(instance, null));
            }

            var asyncMethods = GetMethodsFromHierarchy(
                classHierarchy, conventions.GetAsyncMethodLevelBeforeAll);

            if (asyncMethods.Count > 0)
            {
                AsyncClassHook = instance => asyncMethods.Do(m => new AsyncMethodLevelBeforeAll(m).Run(instance));
            }
        }

        protected override void RunHooks(nspec instance)
        {
            // do NOT traverse parent chain

            // context-level

            if (Hook != null && AsyncHook != null)
            {
                throw new AsyncMismatchException(
                    "A single context cannot set both a 'beforeAll' and an 'beforeAllAsync', please pick one of the two");
            }

            if (Hook != null && Hook.IsAsync())
            {
                throw new AsyncMismatchException(
                    "'beforeAll' cannot be set to an async delegate, please use 'beforeAllAsync' instead");
            }

            Hook.SafeInvoke();

            AsyncHook.SafeInvoke();

            // class (method-level)

            if (ClassHook != null && AsyncClassHook != null)
            {
                throw new AsyncMismatchException(
                    "A spec class with all its ancestors cannot set both sync and async class-level 'before_all' hooks, they should either be all sync or all async");
            }

            ClassHook.SafeInvoke(instance);

            AsyncClassHook.SafeInvoke(instance);
        }

        protected override bool CanRun(nspec instance)
        {
            return AncestorBeforeAllsThrew()
                ? false
                : context.AnyUnfilteredExampleInSubTree(instance);
        }

        public bool AnyBeforeAllsThrew()
        {
            return (Exception != null || AncestorBeforeAllsThrew());
        }

        public bool AncestorBeforeAllsThrew()
        {
            return (context.Parent?.BeforeAllChain.AnyBeforeAllsThrew() ?? false);
        }

        public BeforeAllChain(Context context) : base(context)
        { }
    }
}
