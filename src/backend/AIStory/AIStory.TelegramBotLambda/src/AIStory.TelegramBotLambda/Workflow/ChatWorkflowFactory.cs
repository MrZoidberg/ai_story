namespace AIStory.TelegramBotLambda.Workflow;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

internal class ChatWorkflowFactory
{
    private readonly IServiceProvider serviceProvider;

    public ChatWorkflowFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
/*
    public Workflow InitWorkflow()
    {
        WorkflowBuilder workflowBuilder = new WorkflowBuilder();
        workflowBuilder.WithRootStep()
            .WithName("Start")
            .WithTransition()
            .WithPossibilityCheck((step, context) => true)
            .WithAction(serviceProvider.GetRequiredService<StartCommand>(), (builder) =>
            {
                return builder.WithName("ChooseLanguage").Build();                    
            })
    }*/
}

public interface ICommand
{
    Task<bool> Execute(Message message);
}

public class StartCommand : ICommand
{
    public Task<bool> Execute(Message message)
    {
        throw new NotImplementedException();
    }
}