public class Test2_IfElseChain {

    public static int computeY(double x) {
        int y;

        if (x < 0) {
            y = 0;
        } else if (x == 0) {
            y = 1;
        } else if (x < 0.5) {
            y = 2;
        } else if (x < 1) {
            y = 3;
        } else {
            y = 4;
        }

        return y;
    }

    public static void main(String[] args) {
        double x = 0.7;
        int result = computeY(x);
        System.out.println("Y = " + result);
    }
}