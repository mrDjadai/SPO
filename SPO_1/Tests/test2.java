public class Test3_LoopsAndTernary {

    public static void main(String[] args) {
        int[] data = {3, 7, 1, 9, 4, 6, 8, 2};

        // Ветвление 1: for
        int sum = 0;
        for (int i = 0; i < data.length; i++) {
            sum += data[i];
        }
        System.out.println("Sum: " + sum);

        // Ветвление 2: while
        int max = data[0];
        int j = 1;
        while (j < data.length) {
            // Ветвление 3: тернарный оператор
            max = (data[j] > max) ? data[j] : max;
            j++;
        }
        System.out.println("Max: " + max);

        // Ветвление 4: do-while
        int count = 0;
        int k = 0;
        do {
            // Ветвление 5: тернарный оператор
            count += (data[k] % 2 == 0) ? 1 : 0;
            k++;
        } while (k < data.length);

        System.out.println("Even count: " + count);

        String label = (sum > 0) ? "positive" : "non-positive";
        System.out.println("Label: " + label);
    }
}