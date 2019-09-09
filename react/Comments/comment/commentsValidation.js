import * as Yup from "yup";

const commentSchema = Yup.object().shape({
  text: Yup.string()
    .min(2, "Text must be 2 to 3000 characters")
    .max(3000, "Text must be 2 to 3000 characters")
    .required("Text is required")
});

export default commentSchema;
