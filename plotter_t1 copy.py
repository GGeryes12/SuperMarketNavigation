import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Create DataFrame
data = {
    "Run": [1, 10, 2, 3, 4, 5, 6, 7, 8, 9, 1, 10, 2, 3, 4, 5, 6, 7, 8, 9, 1, 10, 2, 3, 4, 5, 6, 7, 8, 9],
    "Algorithm": ["NSGA2"] * 10 + ["NSGA3"] * 10 + ["SPEA2"] * 10,
    "Hypervolume": [2696512, 2659500, 2544332, 2784128, 2675320, 2571776, 2659500, 2660724, 2737044, 2670620,
                    2554280, 2531140, 2697344, 2591536, 2587788, 2429980, 2637060, 2764744, 2657868, 2775852,
                    2633276, 2594176, 2696360, 2569156, 2733836, 2710536, 2720192, 2669800, 2667156, 2653968],
    "IGD": [98.6, 169.3, 146.0, 39.2, 164.1, 280.3, 169.3, 169.6, 45.5, 22.9,
            163.2, 52.9, 98.8, 202.4, 52.1, 243.2, 163.2, 133.0, 168.8, 36.6,
            273.7, 75.1, 102.7, 88.3, 65.3, 253.1, 133.1, 23.9, 55.7, 29.0],
    "Spread": [8.9, 0.6, 990.4, 109.4, 254.7, 433.1, 3.0, 0.2, 212.7, 764.8,
               417.2, 678.7, 4.9, 314.7, 41.5, 8497.7, 1.8, 260.9, 0.8, 72.9,
               2343.4, 2976.8, 608.2, 1328.8, 810.8, 577.5, 1443.7, 1530.6, 3207.9, 1179.8],
    "Pareto Solutions": [8, 7, 11, 7, 8, 22, 5, 6, 4, 14, 8, 9, 8, 7, 11, 10, 10, 10, 8, 18, 11, 6, 4, 9, 8, 5, 11, 13, 7, 7]
}

df = pd.DataFrame(data)

# Calculate averages per algorithm
avg_results = df.groupby("Algorithm").mean()

# Define metrics
metrics = ["Hypervolume", "IGD", "Spread", "Pareto Solutions"]

# Generate bar charts
for metric in metrics:
    plt.figure(figsize=(8, 5))
    sns.barplot(x=avg_results.index, y=avg_results[metric], palette="viridis")
    plt.xlabel("Algorithm")
    plt.ylabel(metric)
    plt.title(f"Comparison of {metric} Across Algorithms")
    plt.xticks(rotation=45)
    plt.grid(axis="y")
    plt.show()
