public class Test4_SwitchAndNestedIf {

    public static String describe(int code) {
        String result;

        switch (code) {
            case 1:
                result = "one";
                break;
            case 2:
                result = "two";
                break;
            case 3:
                result = "three";
                break;
            case 4:
                result = "four";
                break;
            case 5:
                result = "five";
                break;
            default:
                result = "unknown";
                break;
        }

        return result;
    }

    public static void process(int a, int b) {
        if (a > 0) {
            if (b > 0) {
                System.out.println("both positive");
            } else {
                System.out.println("a positive, b non-positive");
            }
        } else {
            System.out.println("a non-positive");
        }
    }

    public static void main(String[] args) {
        System.out.println(describe(3));
        System.out.println(describe(99));
        process(1, 2);
        process(1, -2);
        process(-1, 0);
    }
}