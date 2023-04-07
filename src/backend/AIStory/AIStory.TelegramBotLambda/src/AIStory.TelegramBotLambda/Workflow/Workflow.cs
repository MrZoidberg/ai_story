namespace AIStory.TelegramBotLambda.Workflow;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

internal class Workflow
{
    public WorkflowStep RootStep { get; set; } = null!;
}

internal class WorkflowStep
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public IEnumerable<WorkflowTransition> Actions { get; set; } = new List<WorkflowTransition>();
}

internal class WorkflowTransition
{
    public Func<Message, WorkflowStep?> Action { get; set; } = null!;

    public Func<WorkflowStep, Message, bool> PossibilityCheck { get; set; } = null!;

    public WorkflowStep Execute(WorkflowStep step, Message context)
    {
        return Action.Invoke(context);
    }

    public bool CanExecute(WorkflowStep step, Message context)
    {
        return PossibilityCheck.Invoke(step, context);
    }
}

internal class WorkflowBuilder
{
    private readonly Workflow workflow;
    private int stepCounter = -1;

    public WorkflowBuilder()
    {
        workflow = new Workflow();
    }
    public WorkflowStepBuilder WithRootStep()
    {
        workflow.RootStep = new WorkflowStep();
        return new WorkflowStepBuilder(this, workflow.RootStep);
    }
    public Workflow Build()
    {
        return workflow;
    }

    internal int NextStepId()
    {
        stepCounter++;
        return stepCounter;
    }
}

internal class WorkflowStepBuilder
{
    private readonly WorkflowBuilder workflowBuilder;
    private readonly WorkflowStep step;

    public WorkflowStepBuilder(WorkflowBuilder workflowBuilder, WorkflowStep step)
    {
        this.workflowBuilder = workflowBuilder;
        this.step = step;
        this.step.Id = workflowBuilder.NextStepId();
    }

    public WorkflowStepBuilder(WorkflowBuilder workflowBuilder)
    {
        this.workflowBuilder = workflowBuilder;
        this.step = new WorkflowStep();
        this.step.Id = workflowBuilder.NextStepId();
    }

    public WorkflowStepBuilder WithName(string name)
    {
        step.Name = name;
        return this;
    }

    public WorkflowTransitionBuilder WithTransition()
    {
        WorkflowTransitionBuilder builder = new WorkflowTransitionBuilder(workflowBuilder, this, step);
        return builder;
    }
    public WorkflowBuilder Workflow()
    {
        return workflowBuilder;
    }

    public WorkflowStep Build()
    {
        return step;
    }
}

internal class WorkflowTransitionBuilder
{
    private readonly WorkflowBuilder workflowBuilder;
    private readonly WorkflowStepBuilder stepBuilder;
    private readonly WorkflowStep parentStep;
    private readonly WorkflowTransition workflowTransition;

    public WorkflowTransitionBuilder(WorkflowBuilder workflowBuilder, WorkflowStepBuilder stepBuilder, WorkflowStep parentStep)
    {
        this.workflowBuilder = workflowBuilder;
        this.stepBuilder = stepBuilder;
        this.parentStep = parentStep;
        workflowTransition = new WorkflowTransition();
    }

    public WorkflowTransitionBuilder WithPossibilityCheck(Func<WorkflowStep, Message, bool> func)
    {
        workflowTransition.PossibilityCheck = func;
        return this;
    }

    public WorkflowStepBuilder WithAction(ICommand command, Func<WorkflowStepBuilder, WorkflowStep> successStepFunc, Func<WorkflowStepBuilder, WorkflowStep>? failStepFunc = null)
    {
        var successStep = successStepFunc(new WorkflowStepBuilder(workflowBuilder));
        var failStep = failStepFunc == null ? null : failStepFunc(new WorkflowStepBuilder(workflowBuilder));

        workflowTransition.Action = (message) =>
        {
            var result = command.Execute(message).GetAwaiter().GetResult();
            if (result)
            {
                return successStep;
            }
            else if (failStep != null)
            {
                return failStep;
            }

            return null;
        };
        return this;
    }

    public WorkflowStepBuilder Step()
    {
        return stepBuilder;
    }
}
