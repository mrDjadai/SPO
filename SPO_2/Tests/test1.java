public class Test1_Linear {

    public static void main(String[] args) {
        int a = 5;
        int b = 10;
        int sum = a + b;
        int product = a * b;
        int diff = b - a;

        System.out.println("Sum: " + sum);
        System.out.println("Product: " + product);
        System.out.println("Diff: " + diff);

        double avg = (a + b) / 2.0;
        System.out.println("Average: " + avg);

        String msg = "Hello, Java";
        System.out.println(msg);

        int[] arr = {1, 2, 3, 4, 5};
        int total = 0;
        total += arr[0];
        total += arr[1];
        total += arr[2];
        total += arr[3];
        total += arr[4];

        System.out.println("Total: " + total);
    }
}