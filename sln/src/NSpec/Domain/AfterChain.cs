using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public class AfterChain : HookChainBase
    {
        public override void BuildMethodLevel(Conventions conventions, List<Type> classHierarchy)
        {
            var methods = GetMethodsFromHierarchy(
                classHierarchy, conventions.GetMethodLevelAfter);

            methods.Reverse();

            if (methods.Count > 0)
            {
                ClassHook = instance => methods.Do(m => m.Invoke(instance, null));
            }

            var asyncMethods = GetMethodsFromHierarchy(
                classHierarchy, conventions.GetAsyncMethodLevelAfter);

            asyncMethods.Reverse();

            if (asyncMethods.Count > 0)
            {
                AsyncClassHook = instance => asyncMethods.Do(m => new AsyncMethodLevelAfter(m).Run(instance));
            }
        }

        protected override void RunHooks(nspec instance)
        {
            // context-level

            if (Hook != null && AsyncHook != null)
            {
                throw new AsyncMismatchException(
                    "A single context cannot set both an 'after' and an 'afterAsync', please pick one of the two");
            }

            if (Hook != null && Hook.IsAsync())
            {
                throw new AsyncMismatchException(
                    "'after' cannot be set to an async delegate, please use 'afterAsync' instead");
            }

            Hook.SafeInvoke();

            AsyncHook.SafeInvoke();

            // class (method-level)

            if (ClassHook != null && AsyncClassHook != null)
            {
                throw new AsyncMismatchException(
                    "A spec class with all its ancestors cannot set both sync and async class-level 'after_each' hooks, they should either be all sync or all async");
            }

            ClassHook.SafeInvoke(instance);

            AsyncClassHook.SafeInvoke(instance);

            // parent chain

            RecurseAncestors(c => c.AfterChain.RunHooks(instance));
        }

        protected override bool CanRun(nspec instance)
        {
            return !context.BeforeAllChain.AnyBeforeAllsThrew();
        }

        public AfterChain(Context context) : base(context)
        { }
    }
}
