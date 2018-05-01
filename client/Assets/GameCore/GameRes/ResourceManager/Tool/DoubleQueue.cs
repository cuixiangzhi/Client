using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Games.TLBB.Manager.Tool
{
    public class DoubleQueue<T>
    {
        private Queue<int> m_Q1;
        private Queue<int> m_Q2;
        private volatile Queue<int> m_CurrentWriteQueue;

        private Thread m_ProducerThread;
        private Thread m_ConsumerThread;

        private ManualResetEvent m_HandlerFinishedEvent;
        private ManualResetEvent m_UnblockHandlerEvent;
        private AutoResetEvent m_DataAvailableEvent;
        private Random m_Random;

        private StreamWriter m_ProducerData;
        private StreamWriter m_ConsumerData;
        private StreamWriter m_Log;

        public DoubleQueue()
        {
            m_Q1 = new Queue<int>();
            m_Q2 = new Queue<int>();

            m_CurrentWriteQueue = m_Q1;

            m_ProducerThread = new Thread(new ThreadStart(ProducerFunc));
            m_ConsumerThread = new Thread(new ThreadStart(ConsumerFunc));

            m_HandlerFinishedEvent = new ManualResetEvent(true);
            m_UnblockHandlerEvent = new ManualResetEvent(true);
            m_DataAvailableEvent = new AutoResetEvent(false);
            m_Random = new Random((int)DateTime.Now.Ticks);

            m_ProducerData = new StreamWriter("Producer.txt");
            m_ProducerData.AutoFlush = true;

            m_ConsumerData = new StreamWriter("Consumer.txt");
            m_ConsumerData.AutoFlush = true;

            m_Log = new StreamWriter("Log.txt");
            m_Log.AutoFlush = true;
        }

        public void Run()
        {
            m_ProducerThread.Start();
            m_ConsumerThread.Start();
        }

        public void ProducerFunc()
        {
            int data = 0;

            for (int i = 0; i < 10000; i++)
            {
                data += 1;
                MessageHandler(data);
                Thread.Sleep(m_Random.Next(0, 2));
            }
        }

        private void MessageHandler(int data)
        {
            m_UnblockHandlerEvent.WaitOne();
            m_HandlerFinishedEvent.Reset();

            m_CurrentWriteQueue.Enqueue(data);
            m_ProducerData.WriteLine(data); // logging 

            m_DataAvailableEvent.Set();
            m_HandlerFinishedEvent.Set();
        }

        public void ConsumerFunc()
        {
            int count;
            int data;
            Queue<int> readQueue;

            while (true)
            {
                m_DataAvailableEvent.WaitOne();

                m_UnblockHandlerEvent.Reset(); // block the producer
                m_HandlerFinishedEvent.WaitOne(); // wait for the producer to finish
                readQueue = m_CurrentWriteQueue;
                m_CurrentWriteQueue = (m_CurrentWriteQueue == m_Q1) ? m_Q2 : m_Q1; // switch the write queue
                m_UnblockHandlerEvent.Set(); // unblock the producer

                count = 0;
                while (readQueue.Count > 0)
                {
                    count += 1;

                    data = readQueue.Dequeue();
                    m_ConsumerData.WriteLine(data); // logging 

                    Thread.Sleep(m_Random.Next(0, 2));
                }
                Console.WriteLine("Removed {0} items from queue: {1}", count, readQueue.GetHashCode());
            }
        }
    }
}
