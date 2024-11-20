namespace CSLangKit;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class EpochMetrics
{
    public int Epoch { get; private set; }
    public DatasetSubset Subset { get; private set; }
    public Dictionary<string, double> Metrics { get; private set; }

    public EpochMetrics(int epoch, DatasetSubset subset, Dictionary<string, double> metrics)
    {
        this.Epoch = epoch;
        this.Subset = subset;
        this.Metrics = metrics;
    }
}

public class AsyncModelTrainer
{
    private IModel<ClassificationExample> Model { get; set; }
    private Dictionary<DatasetSubset, List<ClassificationExample>> Dataset { get; set; }
    private BackgroundWorker Worker { get; set; } =
        new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

    private void TrainContinously(object sender, DoWorkEventArgs e)
    {
        int currentEpoch = 1;
        double currentBestAccuracy = 0;
        double[] currentBestWeights = new double[
            this.Dataset[DatasetSubset.Training].First().Features.Length
        ];

        while (!this.Worker.CancellationPending)
        {
            Random seed = new Random();
            this.Dataset[DatasetSubset.Training].Shuffle(seed);
            this.Dataset[DatasetSubset.Validation].Shuffle(seed);

            // we loop over index wise, fitting a single example at a time to ensure
            // user can cancel and not have to wait for epoch to finish.
            for (int i = 0; i < this.Dataset[DatasetSubset.Training].Count; i++)
            {
                var examples = this.Dataset[DatasetSubset.Training].Head(1, offset: i).ToList();
                this.Model.Fit(examples);
                if (this.Worker.CancellationPending)
                {
                    break;
                }
            }

            var trainingSetMetrics = this.Model.Score(this.Dataset[DatasetSubset.Training]);
            var validationSetMetrics = this.Model.Score(this.Dataset[DatasetSubset.Validation]);

            // save model weights if we have acheived a better validation set accuracy
            bool newBest = validationSetMetrics["accuracy"] > currentBestAccuracy;
            currentBestWeights =
                (newBest) ? this.Model.Weights.Select(w => w).ToArray() : this.Model.Weights;
            this.Model.Weights = currentBestWeights;
            var trainingMetricsReport = new EpochMetrics(
                currentEpoch,
                DatasetSubset.Training,
                trainingSetMetrics
            );
            var validationMetricsReport = new EpochMetrics(
                currentEpoch,
                DatasetSubset.Validation,
                validationSetMetrics
            );
            this.Worker.ReportProgress(0, trainingMetricsReport);
            this.Worker.ReportProgress(0, validationMetricsReport);
            currentEpoch++;
        }

        // functionally equivilent to having a seperate hook for reporting final
        // model test set metrics
        var testSetMetrics = this.Model.Score(this.Dataset[DatasetSubset.Test]);
        var testSetReport = new EpochMetrics(currentEpoch, DatasetSubset.Test, testSetMetrics);
        this.Worker.ReportProgress(0, testSetReport);
    }

    public AsyncModelTrainer(
        IModel<ClassificationExample> model,
        Dictionary<DatasetSubset, List<ClassificationExample>> dataset,
        Action<EpochMetrics> onMetrics
    )
    {
        Model = model;
        Dataset = dataset;
        Worker.DoWork += TrainContinously;
        Worker.ProgressChanged += (object s, ProgressChangedEventArgs e) =>
            onMetrics.Invoke(e.UserState as EpochMetrics);
    }

    public void Train() => Worker.RunWorkerAsync();

    public void StopTraining() => Worker.CancelAsync();
}
