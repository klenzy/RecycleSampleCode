import React from "react";
import PropTypes from "prop-types";
import "./File.css";

const Upload = props => {
  return (
    <>
      <label htmlFor="files">
        <h4 className="files offset-2 fa fa-upload" aria-hidden="true" />
      </label>
      {props.isViewing ? (
        <input
          id="files"
          role="button"
          className="fileInput"
          hidden
          type="file"
          onChange={props.onFileSelector}
        />
      ) : (
        <input
          id="files"
          role="button"
          className="fileInput"
          multiple
          hidden
          type="file"
          onChange={props.onFileSelector}
        />
      )}
    </>
  );
};
Upload.propTypes = {
  onFileSelector: PropTypes.func.isRequired,

  isViewing: PropTypes.bool
};
export default Upload;
