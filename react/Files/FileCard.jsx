import React from "react";
import PropTypes from "prop-types";

const FileCard = props => {
  const onView = () => {
    props.view(props.file);
  };

  return (
    <>
      <div onClick={onView} className="fileIcons">
        <div>{props.file.name}</div>
        <div className="hovereffect">
          <img
            className="card card-img-top img-fluid img-responsive"
            src={props.file.url}
            alt=""
            height="10"
            width="auto"
            style={{ height: "75px", width: "150px" }}
          />

          <div className="overlay">
            <p>View</p>
          </div>
        </div>
      </div>
    </>
  );
};

FileCard.propTypes = {
  file: PropTypes.shape({
    url: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired
  }),
  view: PropTypes.func.isRequired
};

export default FileCard;
