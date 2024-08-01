using UnityEngine;
using LSL;
using System.Collections;

public class LSLReceiverInlets : MonoBehaviour
{
    private string[] streamNames = { "ExperimentPhase", "TimestampsSignaler", "SignalerReady", "BoxSelectedBySignaler", "Rewards", "EyePosDirRotSignaler", "EyeOpennessLRSignaler", "PupilDiameterLRSignaler", "HMDPosDirRotSignaler", "HandPosDirRotSignaler", "PreferredHandSignaler", "FrozenSignaler", "BreakSignaler" };
    private StreamInlet[] streamInlets;
    private int[] channelCounts;
    private int[][] intSamples;
    private float[][] floatSamples;
    private string[][] stringSamples;
    public float sampleInterval = 0.0001f;
    private GameManager gameManager;
    private SignalerManager signalerManager;


    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        signalerManager = GameObject.Find("Signaler").GetComponent<SignalerManager>();

        int streamCount = streamNames.Length;
        streamInlets = new StreamInlet[streamCount];
        channelCounts = new int[streamCount];
        intSamples = new int[streamCount][];
        floatSamples = new float[streamCount][];
        stringSamples = new string[streamCount][];


        StartCoroutine(ResolveAndProcessStreams());
    }
    private IEnumerator ResolveAndProcessStreams()
    {
        while (true)
        {
            for (int i = 0; i < streamNames.Length; i++)
            {
                if (streamInlets[i] == null)
                {
                    ResolveStream(streamNames[i], ref streamInlets[i], ref channelCounts[i]);
                }

                if (streamInlets[i] != null)
                {
                    if (streamInlets[i].info().channel_format() == channel_format_t.cf_float32)
                    {
                        PullAndProcessFloatSample(streamInlets[i], ref floatSamples[i], channelCounts[i], streamNames[i]);
                    }
                    else if (streamInlets[i].info().channel_format() == channel_format_t.cf_int32)
                    {
                        PullAndProcessIntSample(streamInlets[i], ref intSamples[i], channelCounts[i], streamNames[i]);
                    }
                    else if (streamInlets[i].info().channel_format() == channel_format_t.cf_string)
                    {
                        PullAndProcessStringSample(streamInlets[i], ref stringSamples[i], channelCounts[i], streamNames[i]);
                    }
                }
            }

            yield return new WaitForSeconds(sampleInterval);
        }
    }

    private void ResolveStream(string streamName, ref StreamInlet inlet, ref int channelCount)
    {
        StreamInfo[] streamInfos = LSL.LSL.resolve_stream("name", streamName, 1, 0.0);

        if (streamInfos.Length > 0)
        {
            inlet = new StreamInlet(streamInfos[0]);
            channelCount = inlet.info().channel_count();
            inlet.open_stream();
        }
    }

    private void PullAndProcessIntSample(StreamInlet inlet, ref int[] sample, int channelCount, string streamName)
    {
        if (sample == null || sample.Length != channelCount)
        {
            sample = new int[channelCount];
        }

        double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

        if (lastTimeStamp != 0.0)
        {
            ProcessIntSample(sample, lastTimeStamp, streamName);
        }
    }


    private void PullAndProcessFloatSample(StreamInlet inlet, ref float[] sample, int channelCount, string streamName)
    {
        if (sample == null || sample.Length != channelCount)
        {
            sample = new float[channelCount];
        }

        double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

        if (lastTimeStamp != 0.0)
        {
            ProcessFloatSample(sample, lastTimeStamp, streamName);
        }
    }

    private void PullAndProcessStringSample(StreamInlet inlet, ref string[] sample, int channelCount, string streamName)
    {
        if (sample == null || sample.Length != channelCount)
        {
            sample = new string[channelCount];
        }

        double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

        if (lastTimeStamp != 0.0)
        {
            ProcessStringSample(sample, lastTimeStamp, streamName);
        }
    }


    private void ProcessIntSample(int[] sample, double timeStamp, string streamName)
    {
        // Debug.LogWarning($"Received int sample from {streamName} at {timeStamp}: {string.Join(", ", sample)}");

        switch (streamName)

        {
            case "ExperimentPhase":
                // Handle ExperimentPhase stream
                break;
        }
    }

    private void ProcessFloatSample(float[] sample, double timeStamp, string streamName)
    {
        // Debug.LogWarning($"Received float sample from {streamName} at {timeStamp}: {string.Join(", ", sample)}");

        switch (streamName)
        {
            case "SignalerReady":
                // Handle ReceiverReady stream
                break;
            case "HMDPosDirRotSignaler":
                // Handle HMDPosDirRotReceiver stream
                break;
            
            case "BreakSignaler":
                // Handle BreakReceiver stream
                break;
            // Add additional cases for other streams as needed
        }
    }

    private void ProcessStringSample(string[] sample, double timeStamp, string streamName)
    {
        // Debug.LogWarning($"Received string sample from {streamName} at {timeStamp}: {string.Join(", ", sample)}");

        // Example: Handling specific string stream data
        switch (streamName)
        {
            case "ExperimentPhase":
                break;
            // Add additional cases for other streams as needed
            case "FrozenSignaler":
                Debug.Log(sample[0]);
                signalerManager.frozen = sample[0]=="true";
                break;
        }
    }
}
