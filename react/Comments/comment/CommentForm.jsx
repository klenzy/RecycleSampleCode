import React, { Component } from "react";
import logger from "sabio-debug";
import commentSchema from "./commentsValidation";
import * as commentService from "../../services/commentService";
import { Col, Card, FormGroup } from "reactstrap";
import { Formik, Field, Form } from "formik";
import PropTypes from "prop-types";
import "./Comment.css";
import swal from "sweetalert";

const _logger = logger.extend("comments");

class CommentForm extends Component {
  constructor(props) {
    super(props);
    this.state = {
      comment: {
        id: 0,
        subject: "",
        text: "",
        parentId: 0
      }
    };
  }

  componentDidMount() {
    if (this.props.comment && this.props.isEditing) {
      const data = { ...this.props.comment };
      this.setState({
        comment: {
          id: data.id,
          subject: data.subject,
          entityId: this.props.entityId,
          entityTypeId: this.props.entityTypeId,
          text: data.text,
          parentId: data.parentId
        }
      });
    }
  }

  commentData = data => {
    if (this.props.comment && !this.props.isEditing) {
      return {
        comment: {
          subject: data.subject,
          text: data.text,
          parentId: this.props.comment.id,
          entityId: this.props.entityId,
          entityTypeId: this.props.entityTypeId
        }
      };
    } else if (this.props.isEditing) {
      const commentData = data;
      return {
        comment: {
          id: this.props.comment.id,
          subject: commentData.subject,
          text: commentData.text,
          parentId: this.props.comment.parentId,
          entityId: this.props.entityId,
          entityTypeId: this.props.entityTypeId
        }
      };
    } else {
      return {
        comment: {
          subject: data.subject,
          text: data.text,
          entityId: this.props.entityId,
          entityTypeId: this.props.entityTypeId
        }
      };
    }
  };

  handleSubmit = formData => {
    _logger(formData);
    this.setState(() => {
      return this.commentData(formData);
    });
    if (!this.props.isEditing) {
      commentService
        .add(this.state.comment)
        .then(this.submitSuccess)
        .then(this.props.comment ? this.setFalse : null)
        .catch(this.submitError);
    } else {
      commentService
        .update(this.state.comment)
        .then(this.submitSuccess)
        .then(this.props.comment ? this.setFalse : null)
        .catch(this.submitError);
    }
  };

  resetForm = () => {
    return {
      comment: {
        id: 0,
        subject: "",
        text: "",
        parentId: 0
      }
    };
  };

  submitSuccess = () => {
    this.props.comment || this.props.isEditing
      ? this.props.getAll()
      : this.props.submitSuccess();

    this.setState(res => {
      return this.resetForm(res);
    });
  };

  setFalse = () => {
    this.props.setFalse();
  };

  submitError = () => {
    swal({
      title: "Failed to add comment!",
      text: "Please make sure you are logged in!",
      icon: "error"
    });
  };

  render() {
    return (
      <>
        <div
          className={`mt-3 form card-default cardform card ${
            this.props.currentUser.id > 0 ? null : "hidden"
          }`}
        >
          <Col>
            <Card>
              <FormGroup />
              {!this.props.comment && !this.props.isEditing ? (
                <h3 className="offset-.5"> Add a Comment</h3>
              ) : null}
              <Formik
                initialValues={this.state.comment}
                enableReinitialize={true}
                onSubmit={this.handleSubmit}
                validationSchema={commentSchema}
                render={formikProps => (
                  <Form>
                    <FormGroup>
                      <Field
                        type="text"
                        name="subject"
                        className="form-control"
                        placeholder="Topic"
                      />
                      {formikProps.touched.subject &&
                        formikProps.errors.subject && (
                          <div className="text-danger">
                            {formikProps.errors.subject}
                          </div>
                        )}
                    </FormGroup>

                    <FormGroup>
                      <Field
                        component="textarea"
                        name="text"
                        className="form-control"
                        placeholder="What would you like to say?"
                      />
                      {formikProps.touched.text && formikProps.errors.text && (
                        <div className="text-danger">
                          {formikProps.errors.text}
                        </div>
                      )}
                    </FormGroup>

                    <FormGroup className="float-right">
                      {" "}
                      <button type="submit" className="btn btn-dark" size="md">
                        {this.props.isEditing ? "Update" : "Submit"}
                      </button>
                    </FormGroup>
                  </Form>
                )}
              />
            </Card>
          </Col>
        </div>
      </>
    );
  }
}

CommentForm.propTypes = {
  comment: PropTypes.shape({
    id: PropTypes.number.isRequired,
    subject: PropTypes.string,
    text: PropTypes.string.isRequired,
    parentId: PropTypes.number
  }),
  currentUser: PropTypes.object,
  submitSuccess: PropTypes.func,
  setFalse: PropTypes.func,
  getAll: PropTypes.func,
  isEditing: PropTypes.bool,
  entityId: PropTypes.number,
  entityTypeId: PropTypes.number
};
export default CommentForm;
