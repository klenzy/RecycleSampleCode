import React, { Component } from "react";
import logger from "sabio-debug";
import Comment from "./Comment";
import PropTypes from "prop-types";
import CommentForm from "./CommentForm";
import * as commentService from "../../services/commentService";
import * as userService from "../../services/userService";
import swal from "sweetalert";

const _logger = logger.extend("comments");

class Comments extends Component {
  constructor(props) {
    super(props);

    this.state = {
      currentUser: {},
      comments: [],
      mappedComments: []
    };
  }

  componentDidMount() {
    this.getAll();
    this.checkCurrentUser();
  }

  getAll = () => {
    commentService
      .getAll(this.props.entityId, this.props.entityTypeId)
      .then(this.onGetSuccess)
      .catch(this.onGetError);
  };

  delete = id => {
    commentService.deleteById(id);
  };

  mapComment = comment => (
    <Comment
      comment={comment}
      current={this.state.currentUser}
      key={comment.id}
      getAll={this.getAll}
      deleteById={this.deleteById}
      entityId={this.props.entityId}
      entityTypeId={this.props.entityTypeId}
    />
  );

  onGetSuccess = response => {
    const comments = response.item;
    this.setState({
      comments,
      mappedComments: comments.map(this.mapComment)
    });
  };

  onGetError = response => {
    _logger(response, "No comments for this page yet!");
  };

  checkCurrentUser = () => {
    userService.checkAuth().then(this.onCurrentUserSuccess);
  };

  onCurrentUserSuccess = response => {
    _logger(response);

    this.setState({
      currentUser: response.item
    });
  };

  submitSuccess = () => {
    this.getAll();
    _logger("success");
  };

  deleteById = id => {
    swal({
      title: "Are you sure?",
      text: "You will also be deleting the replies!",
      icon: "warning",
      buttons: true,
      dangerMode: true
    }).then(willDelete => {
      if (willDelete) {
        commentService
          .deleteById(id)
          .then(() => {
            this.onDeleteSuccess(id);
          })
          .catch(this.onError);
        swal({
          title: "Your comment has been deleted!",
          icon: "success"
        });
      }
    });
  };

  onDeleteSuccess = id => {
    _logger(id);
    this.getAll();
  };

  render() {
    return (
      <>
        <div className="row">
          <div className="col-md-6 float-left">
            {this.state.mappedComments}
            {this.state.currentUser.id > 0 ? (
              <CommentForm
                submitSuccess={this.submitSuccess}
                entityId={this.props.entityId}
                entityTypeId={this.props.entityTypeId}
                currentUser={this.state.currentUser}
              />
            ) : null}
            <h4
              className={`mt-2 ${
                this.state.currentUser.id > 0 ? "hidden" : null
              }`}
            >
              Please sign in to write a comment!
            </h4>
          </div>
        </div>
      </>
    );
  }
}
Comments.propTypes = {
  currentUser: PropTypes.object,
  history: PropTypes.object,
  match: PropTypes.object,
  location: PropTypes.object,
  entityId: PropTypes.number.isRequired,
  entityTypeId: PropTypes.number.isRequired
};

export default Comments;
