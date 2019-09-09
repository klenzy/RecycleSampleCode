import React, { useState } from "react";
import PropTypes from "prop-types";
import * as dateService from "../../services/dateService";
import "./Comment.css";
import CommentForm from "./CommentForm";

const Comment = props => {
  const [reply, setReply] = useState(false);
  const [edit, setEdit] = useState(false);

  const recursiveComment =
    props.comment.replies &&
    props.comment.replies.length > 0 &&
    props.comment.replies.map(reply => {
      return (
        <Comment
          comment={reply}
          key={reply.id}
          current={props.current}
          getAll={props.getAll}
          deleteById={props.deleteById}
          entityId={props.entityId}
          entityTypeId={props.entityTypeId}
        />
      );
    });

  const getAll = () => {
    props.getAll(getAll);
  };

  const setFalse = () => {
    setReply(false);
    setEdit(false);
  };

  const deleteById = () => {
    props.deleteById(props.comment.id);
  };

  return (
    <>
      <div className="mt-2 card-body img-fluid commentbox">
        <div className="media">
          <img
            className="mr-4 rounded-circle profile"
            src={props.comment.createdBy.avatarUrl}
            alt="Demo"
            height="80"
            width="80"
          />

          <div className="media-body">
            <h4 id="media-heading">
              {" "}
              {props.comment.createdBy.firstName}
              {"   " + props.comment.createdBy.lastName + "  "}
              <small className="text-align center">
                |{" "}
                {dateService
                  .formatTime(props.comment.dateCreated)
                  .substring(
                    0,
                    dateService.formatTime(props.comment.dateCreated).length - 3
                  )}
                -{"       "}
                {dateService
                  .formatDate(props.comment.dateCreated)
                  .substring(
                    0,
                    dateService.formatDate(props.comment.dateCreated).length - 6
                  )}
              </small>
              {props.comment.dateCreated !== props.comment.dateModified ? (
                <small>
                  {" "}
                  <i className="offset-1">(edited)</i>
                </small>
              ) : null}
            </h4>
            {edit ? (
              <div className={`edit`}>
                <CommentForm
                  setFalse={setFalse}
                  comment={props.comment}
                  currentUser={props.current}
                  getAll={getAll}
                  isEditing={edit}
                  entityId={props.entityId}
                  entityTypeId={props.entityTypeId}
                />
              </div>
            ) : (
              <div>
                <strong>{props.comment.subject}</strong>
                <p>{props.comment.text}</p>
              </div>
            )}

            {reply ? (
              <div className={`reply`}>
                <CommentForm
                  setFalse={setFalse}
                  comment={props.comment}
                  currentUser={props.current}
                  getAll={getAll}
                  entityId={props.entityId}
                  entityTypeId={props.entityTypeId}
                />
              </div>
            ) : null}
            {reply ? (
              <span
                onClick={() => {
                  setReply(false);
                }}
                className="reply "
              >
                Close
              </span>
            ) : (
              <span
                onClick={() => {
                  setReply(true);
                }}
                className={`${props.current.id > 0 ? null : "hidden"} ${
                  edit ? "hidden " : null
                } reply fa fa-reply`}
              >
                {" "}
                <span className={`wordReply `}>Reply</span>
              </span>
            )}
            {"   "}
            <span
              className={`${
                props.current.id === props.comment.createdBy.userId
                  ? null
                  : "hidden"
              }`}
            >
              {" "}
              <span className="edit offset-3">
                {edit ? (
                  <span
                    onClick={() => {
                      setEdit(false);
                    }}
                    className="fa-fw fa-sm mr-2"
                  >
                    Cancel
                  </span>
                ) : (
                  <i
                    onClick={() => {
                      setEdit(true);
                    }}
                    className={`${
                      reply ? "hidden " : null
                    }fa-fw fa-sm fa fa-edit mr-2`}
                  />
                )}
              </span>
              <span onClick={deleteById} className="delete">
                <i className="fa-fw fa-sm fas fa-trash-alt mr-2" />
              </span>
            </span>
          </div>
        </div>
        <div className="replyBox"> {recursiveComment}</div>
      </div>
    </>
  );
};

Comment.propTypes = {
  comment: PropTypes.shape({
    id: PropTypes.number.isRequired,
    subject: PropTypes.string,
    text: PropTypes.string.isRequired,
    dateCreated: PropTypes.string.isRequired,
    dateModified: PropTypes.string.isRequired,
    replies: PropTypes.arrayOf(PropTypes.shape({})),
    createdBy: PropTypes.shape({
      userId: PropTypes.number.isRequired,
      firstName: PropTypes.string.isRequired,
      lastName: PropTypes.string.isRequired,
      avatarUrl: PropTypes.string.isRequired
    })
  }),

  entityId: PropTypes.number,
  entityTypeId: PropTypes.number,
  getAll: PropTypes.func.isRequired,
  current: PropTypes.object.isRequired,
  submitSuccess: PropTypes.func,
  submitSuccessReply: PropTypes.func,
  deleteById: PropTypes.func.isRequired
};

export default Comment;
