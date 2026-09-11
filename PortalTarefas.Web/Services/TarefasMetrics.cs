using System.Diagnostics.Metrics;

namespace PortalTarefas.Web.Services;

public class TarefasMetrics
{
    public const string MeterName = "PortalTarefas.Metrics";
    private readonly Counter<long> _tarefasCriadasCounter;

    public TarefasMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _tarefasCriadasCounter = meter.CreateCounter<long>(
            "portal_tarefas_criadas_total",
            description: "Métrica de negócio: Total de tarefas criadas na aplicação.");
    }

    public void RegistrarTarefaCriada(string prioridade)
    {
        _tarefasCriadasCounter.Add(1, new KeyValuePair<string, object?>("prioridade", prioridade));
    }
}