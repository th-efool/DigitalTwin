using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace realvirtual
{
    
    public class Distribution
    {
        public enum DistributionType {Const,Uniform, Erlang, Normal, Exponential};

        public float Min;
        public float Max;
        public float Mean;
        public float StandardDeviation;
        public float Rate;
        public int Shape;
        public DistributionType Type;
        public int Seed;
        private ContinuousUniform uniform;
        private Normal normal;
        private Erlang erlang;
        private Exponential exponential;
        private System.Random random;
        
        
        
        public void Init()
        {
            random = new SystemRandomSource(Seed);
            switch (Type)
            {
              case (DistributionType.Uniform) : 
                  uniform = new ContinuousUniform(Min,Max,random);
                  break;
              case (DistributionType.Normal) :
                  normal = new Normal(Mean,StandardDeviation,random);
                  break;
              case (DistributionType.Erlang) :
                  erlang = new Erlang(Shape,Rate,random);
                  break;
              case (DistributionType.Exponential) :
                  exponential = new Exponential(Rate, random);
                  break;
            }
        }

        public float GetNextRandom()
        {
            float sample = 0f;
            switch (Type)
            {
                case (DistributionType.Uniform) : 
                    sample = (float)uniform.Sample();
                    break;
                case (DistributionType.Normal) :
                    sample = (float)normal.Sample();
                    break;
                case (DistributionType.Erlang) :
                    sample = (float)erlang.Sample();
                    break;
                case (DistributionType.Exponential) :
                    sample = (float)exponential.Sample();
                    break;
            }
            return sample;
        }
    }

}
